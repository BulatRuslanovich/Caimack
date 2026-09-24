using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using App.Abstraction;
using App.Options;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public class JwtTokenService(IOptions<JwtOptions> ops, TimeProvider time) : ITokenService
{
	public IssuedToken CreateAToken(User user)
	{
		var now = time.GetUtcNow();
		var expiresAt = now.AddMinutes(10);

		var claims = new Dictionary<string, object>
		{
			["sub"] = user.Id.ToString(),
			["username"] = user.Username,
			["jti"] = Guid.CreateVersion7().ToString("N"),
		};

		if (user.IsAdmin) claims["role"] = "Admin";

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ops.Value.SigningKey));

		var desc = new SecurityTokenDescriptor
		{
			Issuer = "Caimack",
			Audience = "Caimack",
			IssuedAt = now.UtcDateTime,
			NotBefore = now.UtcDateTime,
			Expires = expiresAt.UtcDateTime,
			Claims = claims,
			SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
		};

		var token = new JsonWebTokenHandler().CreateToken(desc);
		return new IssuedToken(token, expiresAt);
	}

	public IssuedRToken CreateRToken(Guid id)
	{
		var now = time.GetUtcNow();
		var raw = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));

		var entity = new RefreshToken()
		{
			UserId = id,
			TokenHash = HashRToken(raw),
			CreateAt = now,
			ExpiresAt = now.AddDays(30)
		};

		return new IssuedRToken(raw, entity);
	}

	public string HashRToken(string value)
	{
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
	}
}

public class CPCurrentUser(ClaimsPrincipal principal) : ICurrentUser
{
	public Guid Id
	{
		get
		{
			var value = principal.FindFirst("sub")?.Value;
			return Guid.TryParse(value, out var id) ? id : Guid.Empty;
		}
	}

	public bool IsAuthenticated => principal.Identity?.IsAuthenticated == true && Id != Guid.Empty;
}
