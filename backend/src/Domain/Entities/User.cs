namespace Domain.Entities;

public class User
{
	public Guid Id { get; set; } = Guid.CreateVersion7();
	public string Username { get; set; } = string.Empty;
	public string PasswordHash { get; set; } = string.Empty;
	public bool IsAdmin { get; set; }
	public bool IsActive { get; set; } = true;
	public DateTimeOffset CreatedAt { get; set; }
}
