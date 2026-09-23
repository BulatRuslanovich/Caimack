namespace App.Dtos;

public record AuthUserDto(
		Guid Id,
		string Username,
		bool IsAdmin,
		bool IsActive,
		DateTimeOffset  CreatedAt
	);

public record CreateUserDto(
		string Username,
		string Password,
		bool IsAdmin
	);

public record SetActiveDto(bool IsActive);

public record SetRoleDto(bool IsAdmin);

public record ResetPassDto(string NewPass);


