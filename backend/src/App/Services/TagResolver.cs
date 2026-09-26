using App.Abstraction;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Services;

public class TagResolver(IAppDbContext db, TimeProvider time)
{
	private readonly Dictionary<string, Artist> _artists = new(StringComparer.Ordinal);
	private readonly Dictionary<string, Genre> _genres = new(StringComparer.Ordinal);
	private readonly Dictionary<(Guid ArtistId, string Key), Album> _albums = [];
	private readonly Dictionary<string, bool> _knownNames = new(StringComparer.Ordinal);

	public async Task<IReadOnlyList<Artist>> ResolveArtistsAsync(IEnumerable<string?> rawVals, CancellationToken ct)
	{
		var res = new List<Artist>();
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var raw in rawVals)
		{
			if (string.IsNullOrWhiteSpace(raw)) continue;

			IReadOnlyList<string> names;
			var key = Normalize.Key(raw);
			if (_artists.ContainsKey(key)) names = [raw];
			else
			{
				if (!_knownNames.TryGetValue(key, out var known))
				{
					known = await db.Artists.AnyAsync(a => a.NormalizedName == key, ct);
					_knownNames[key] = known;
				}

				names = known ? [raw] : ArtistNames.Split(raw);
			}

			foreach (var name in names)
			{
				if (!seen.Add(Normalize.Key(name))) continue;

				res.Add(await GetOrCreateArtistAsync(name, ct));
				if (res.Count == 12) return res;
			}
		}

		return res.Count > 0 ? res : [await GetOrCreateArtistAsync("Unknown", ct)];
	}

	private async Task<Artist> GetOrCreateArtistAsync(string name, CancellationToken ct)
	{
		var trim = string.IsNullOrWhiteSpace(name) ? "Unknow" : name.Trim();
		var key = Normalize.Key(name);

		if (_artists.TryGetValue(key, out var cached))
		{
			return cached;
		}

		var artist = await db.Artists.FirstOrDefaultAsync(a => a.NormalizedName == key, ct);

		if (artist == null)
		{
			artist = new Artist
			{
				Name = trim,
				NormalizedName = key,
				CreatedAt = time.GetUtcNow()
			};

			db.Artists.Add(artist);
		}

		_artists[key] = artist;
		return artist;
	}

	public async Task<Album> GetOrCreateAlbum(
		string title,
		Guid artistId,
		int? year,
		CancellationToken ct)
	{
		var trim = string.IsNullOrWhiteSpace(title) ? "Unknow" : title.Trim();
		var key = Normalize.Key(trim);

		if (_albums.TryGetValue((artistId, key), out var cached))
		{
			cached.Year ??= year;
			return cached;
		}

		
	}
}
