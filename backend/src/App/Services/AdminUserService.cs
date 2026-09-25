using System.Text.RegularExpressions;
using App.Abstraction;
using App.Common;
using App.DTOs;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = App.Common.ValidationException;

namespace App.Services;

public partial class AdminUserService(IAppDbContext db, IPassHasher hasher, TimeProvider time, ILogger<AdminUserService> log)
{
	[GeneratedRegex("^[a-z0-9][a-z0-9._-]{4,19}$")]
	private static partial Regex UsernamePattern();

	public async Task<PagedResult<AuthUserDto>> GetUsersAsync(PageRequest page, CancellationToken ct)
	{
		return await db.Users.AsNoTracking()
			.OrderBy(u => u.Username)
			.ToPagedAsync(page, ToDto.AuthUser, ct);
	}

	public async Task<AuthUserDto> CreateUserAsync(CreateUserDto dto, CancellationToken ct)
	{
		var username = Normalize.Username(dto.Username);

		if (!UsernamePattern().IsMatch(username))
		{
			throw new ValidationException("Username must be 5-20 char of lowercase letters, digits, dot, dash or underscore.");
		}

		var password = PasswordValidator.Validate(dto.Password);

		var user = new User
		{
			Username = username,
			PasswordHash = hasher.Hash(password),
			IsAdmin = dto.IsAdmin,
			CreatedAt = time.GetUtcNow(),
		};

		db.Users.Add(user);

		try
		{
			await db.SaveChangesAsync(ct).ConfigureAwait(false);
		}
		catch (DbUpdateException)
		{
			throw new ConflictException();
		}

		return ToDto.FromUserToAuth(user);
	}

	public async Task<AuthUserDto> SetActiveAsync(Guid id, bool active, CancellationToken ct)
	{
		var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct) ?? throw new NotFoundException();

		user.IsActive = active;

		if (!active)
		{
			await db.RefreshTokens.Where(t => t.UserId == id && t.RevokedAt == null)
				.ExecuteUpdateAsync(t => t.SetProperty(tk => tk.RevokedAt, time.GetUtcNow()), ct);
		}

		await db.SaveChangesAsync(ct).ConfigureAwait(false);
		return ToDto.FromUserToAuth(user);
	}

	public async Task<AuthUserDto> SetRoleAsync(Guid id, bool isAdmin, CancellationToken ct)
	{
		var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct) ?? throw new NotFoundException();

		user.IsAdmin = isAdmin;

		await db.SaveChangesAsync(ct).ConfigureAwait(false);

		return ToDto.FromUserToAuth(user);
	}

	public async Task ResetPassAsync(Guid id, string password, CancellationToken ct)
	{
		var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct) ?? throw new NotFoundException();
		user.PasswordHash = hasher.Hash(PasswordValidator.Validate(password));

		await db.RefreshTokens.Where(t => t.UserId == id && t.RevokedAt == null)
			.ExecuteUpdateAsync(t => t.SetProperty(tk => tk.RevokedAt, time.GetUtcNow()), ct);

		await db.SaveChangesAsync(ct).ConfigureAwait(false);
	}

	public async Task RevokeAsync(Guid id, CancellationToken ct)
	{
		await db.RefreshTokens.Where(t => t.UserId == id && t.RevokedAt == null)
			.ExecuteUpdateAsync(t => t.SetProperty(tk => tk.RevokedAt, time.GetUtcNow()), ct);
		await db.SaveChangesAsync(ct).ConfigureAwait(false);
	}
}
