using App.Abstraction;

namespace Infrastructure.Security;

public class BCryptPassHasher : IPassHasher
{
	public string Hash(string password)
	{
		return BCrypt.Net.BCrypt.HashPassword(password, 12);
	}

	public bool Verify(string password, string hash)
	{
		try
		{
			return BCrypt.Net.BCrypt.Verify(password, hash);
		}
		catch (BCrypt.Net.SaltParseException)
		{
			return false;
		}
	}
}
