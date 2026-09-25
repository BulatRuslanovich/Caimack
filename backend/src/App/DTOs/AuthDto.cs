namespace App.DTOs;

public record AuthUserDto(
		Guid Id,
		string Username,
		bool IsAdmin,
		bool IsActive,
		DateTimeOffset CreatedAt
	);

public record CreateUserDto(
		string Username,
		string Password,
		bool IsAdmin
	);

public record SetActiveDto(bool IsActive);

public record SetRoleDto(bool IsAdmin);

public record ResetPassDto(string NewPass);

public record UserDto(Guid Id, string  Username, bool IsAdmin);

public record AuthResponseDto(
	UserDto User,
	string AToken,
	DateTimeOffset ATokenExpiresAt,
	string RToken,
	DateTimeOffset RTokenExpiresAt
);

public record LoginDto(
	string Username,
	string Password
);

public record ChangePassDto(string currentPass, string newPass);


