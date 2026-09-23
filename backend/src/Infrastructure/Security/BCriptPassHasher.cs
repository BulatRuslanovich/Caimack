using App.Abstraction;

namespace Infrastructure.Security;

public class BCryptPassHasher : IPassHasher
{
	public string Hash(string password)
	{
		return BCrypt.Net.BCrypt.HashPassword(password);
	}

	public bool Verify(string password, string hash)
	{
		return  BCrypt.Net.BCrypt.Verify(password, hash);
	}
}
