using System.Text.RegularExpressions;

namespace Domain.Common;

public static partial class ArtistNames
{
	[GeneratedRegex(
		@"\s*[;/]\s*|\s*,\s*|[\s(\[]+(?:featuring|feat|ft|versus|vs)\.?\s+|\s+[x×]\s+",
		RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	private static partial Regex SeparatorPattern();

	public static IReadOnlyList<string> Split(string? raw)
	{
		if (string.IsNullOrWhiteSpace(raw)) return [];
		var cleaned = raw.Replace("\0", string.Empty).Trim(' ', '\t', '(', ')', '[', ']', '-', '_');
		if (cleaned.Length == 0) return [];

		var names = new List<string>();
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var part in SeparatorPattern().Split(cleaned))
		{
			var name = part.Replace("\0", string.Empty).Trim(' ', '\t', '(', ')', '[', ']', '-', '_');

			if (name.Length == 0 || !seen.Add(Normalize.Key(name))) continue;
			names.Add(name);
			if (names.Count == 12) break;
		}

		return names.Count == 0 ? [cleaned] : names;
	}

	public static IReadOnlyList<string> SplitAll(IEnumerable<string?> raws)
	{
		var names = new List<string>();
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var name in raws.SelectMany(Split))
		{
			if (!seen.Add(Normalize.Key(name))) continue;

			names.Add(name);
			if (names.Count == 12) break;
		}

		return names;
	}
}
