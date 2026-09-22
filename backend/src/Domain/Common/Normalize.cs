namespace Domain.Common;

public static class Normalize
{
	public static string Key(string value)
	{
		return string.Join(' ',
			value.Trim().ToLowerInvariant()
				.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
	}

	public static string Username(string value)
	{
		return string.Join(' ',
			value.Trim().ToLowerInvariant());
	}
}
