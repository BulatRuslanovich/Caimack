namespace App.Abstraction;

public interface IPassHasher
{
	string Hash(string password);
	bool Verify(string password, string hash);
}
