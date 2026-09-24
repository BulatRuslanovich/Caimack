namespace App.Abstraction;

public interface ICurrentUser
{
	Guid Id  { get; }
	bool IsAuthenticated { get; }
}
