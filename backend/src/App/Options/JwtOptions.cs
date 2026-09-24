using System.Text;
using Microsoft.Extensions.Options;

namespace App.Options;

public class JwtOptions
{
	public const string SectionName = "Jwt";
	public string SigningKey { get; set; } = string.Empty;

	public static OptionsBuilder<JwtOptions> Validator(OptionsBuilder<JwtOptions> b)
	{
		return b.Validate(l => !string.IsNullOrWhiteSpace(l.SigningKey), "SigningKey is required")
			.Validate(l => Encoding.UTF8.GetByteCount(l.SigningKey) >= 32, "SigningKey must be at least 32 bytes long");
	}
}
