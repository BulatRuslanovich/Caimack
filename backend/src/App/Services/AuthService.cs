using App.Abstraction;
using App.Common;
using App.Dtos;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace App.Services;

public class AuthService(IAppDbContext db, ITokenService token, IPassHasher hasher, TimeProvider time, LoginAttemptTracker tracker)
{
	public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct)
	{
		var username = Normalize.Username(dto.Username);

		if (tracker.LockoutRemaining(username) is { } remaining)
		{
			throw new ForbiddenException();
		}

		var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
		var validPass = hasher.Verify(dto.Password, user?.PasswordHash);

		if (user is null || !validPass)
		{
			tracker.WriteFail(username);
			throw new ForbiddenException();
		}

		if (!user.IsActive)
		{
			throw new ForbiddenException();
		}

		tracker.WriteSuccess(username);

		var atoken = token.CreateAToken(user);
		var rtoken = token.CreateRToken(user.Id);

		db.RefreshTokens.Add(rtoken.Entity);

		var dayAgo = time.GetUtcNow().AddDays(-1);

		var forDelete = await db.RefreshTokens.Where(t =>
				t.UserId == user.Id && (t.ExpiresAt < dayAgo || (t.RevokedAt != null && t.RevokedAt < dayAgo)))
			.ToListAsync(ct);

		if (forDelete.Count > 0) db.RefreshTokens.RemoveRange(forDelete);

		await db.SaveChangesAsync(ct);

		return new AuthResponseDto(
			ToDto.FromUser(user),
			atoken.Value,
			atoken.ExpiresAt,
			rtoken.RawValue,
			rtoken.Entity.ExpiresAt
		);
	}
}
