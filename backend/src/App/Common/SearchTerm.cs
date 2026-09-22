using Domain.Common;

namespace App.Common;

public readonly record struct SearchTerm(string Value, string Pattern)
{
	public const string EscapeChar = "\\";

	public static SearchTerm? For(string? query)
	{
		var value = Normalize.Key(query ?? string.Empty);
		return value.Length == 0 ? null : new SearchTerm(value, $"%{value.Replace("\\", @"\\").Replace("%", "\\%").Replace("_", "\\_")}%");
	}

	public static SearchTerm? ForSearch(string? query)
	{
		return For(query) is { } term && term.Value.Length >= 3 ? term : null;
	}
}
