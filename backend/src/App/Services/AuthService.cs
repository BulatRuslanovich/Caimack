using System.Security.Authentication;
using App.Abstraction;
using App.Common;
using App.DTOs;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;

namespace App.Services;

public class AuthService(IAppDbContext db, ITokenService token, IPassHasher hasher, TimeProvider time, LoginAttemptTracker tracker)
{
	private async Task<AuthResponseDto> IssueAsync(User user, CancellationToken ct)
	{
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

	public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct)
	{
		var username = Normalize.Username(dto.Username);

		if (tracker.LockoutRemaining(username) is not null)
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

		if (!user.IsActive) throw new ForbiddenException();

		tracker.WriteSuccess(username);
		return await IssueAsync(user, ct);
	}

	public async Task<AuthResponseDto> RefreshAsync(string raw, CancellationToken ct)
	{
		var hash = token.HashRToken(raw);
		var now = time.GetUtcNow();

		var tk = await db.RefreshTokens
			.Include(t => t.User)
			.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

		if (tk is { RevokedAt: { } revokedAt })
		{
			var lives = await db.RefreshTokens
				.Where(t => t.UserId == tk.UserId && t.RevokedAt == null && t.ExpiresAt > now)
				.AnyAsync(ct);

			if (!lives || now - revokedAt > TimeSpan.FromSeconds(20))
			{
				await db.RefreshTokens.Where(t => t.UserId == tk.UserId && t.RevokedAt == null)
					.ExecuteUpdateAsync(t => t.SetProperty(tkn => tkn.RevokedAt, now), ct);

				throw new AuthenticationException();
			}
		}

		if (tk?.User is null || tk.ExpiresAt <= now || !tk.User.IsActive) throw new AuthenticationException();

		tk.RevokedAt = now;
		return await IssueAsync(tk.User, ct);
	}

	public async Task LogoutAsync(string raw, CancellationToken ct)
	{
		var hash = token.HashRToken(raw);
		var tk = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

		if (tk is { RevokedAt: null })
		{
			tk.RevokedAt = time.GetUtcNow();
			await db.SaveChangesAsync(ct);
		}
	}

	public async Task<AuthResponseDto> ChangePasswordAsync(ChangePassDto dto, Guid id,  CancellationToken ct)
	{
		var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct) ?? throw new NotFoundException();

		if (!hasher.Verify(dto.currentPass, user.PasswordHash)) throw new ForbiddenException();

		var pass = PasswordValidator.Validate(dto.newPass);

		if (hasher.Verify(pass, user.PasswordHash)) throw new ValidationException();

		user.PasswordHash = hasher.Hash(pass);
		var now = time.GetUtcNow();
		await db.RefreshTokens.Where(t => t.UserId == user.Id && t.RevokedAt == null)
			.ExecuteUpdateAsync(t => t.SetProperty(tkn => tkn.RevokedAt, now), ct);

		return await IssueAsync(user, ct);
	}

	public async Task<UserDto> GetMeAsync(Guid id, CancellationToken ct)
	{
		var user = await db.Users
			.Where(u => u.Id == id)
			.Select(ToDto.User)
			.FirstOrDefaultAsync(ct);

		return user ?? throw new NotFoundException();

	}


}
