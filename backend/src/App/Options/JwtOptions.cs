namespace App.Options;

public class JwtOptions
{
	public const string SectionName = "Jwt";
	public string SigningKey { get; set; } = string.Empty;
}
