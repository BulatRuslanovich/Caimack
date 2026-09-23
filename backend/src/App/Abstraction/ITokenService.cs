using Domain.Entities;

namespace App.Abstraction;

public record IssuedToken(string Value, DateTimeOffset ExpiresAt);

public record IssuedRToken(string RawValue, RefreshToken Entity);

public interface ITokenService
{
	IssuedToken CreateAToken(User user);
	IssuedRToken CreateRToken(Guid id);
	string HashRToken(string value);
}
