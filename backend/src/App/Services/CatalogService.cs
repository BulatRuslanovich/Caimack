using App.Abstraction;
using App.Common;
using App.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Services;

public class CatalogService(IAppDbContext db)
{
	public enum TrackSort { Title, Recent, Artist, Album }


    public async Task<PagedResult<TrackDto>> GetTracksAsync(PageRequest page, TrackSort sort, string? search, CancellationToken ct)
    {
	    var query = FilteredTracks(search);

	    var ordered = sort switch
	    {
		    TrackSort.Recent => query.OrderByDescending(t => t.Id),
		    _ => query.OrderBy(t => t.Title)
	    };

        return await ordered.ToPagedAsync(page, ToDto.Track(Guid.CreateVersion7()), ct);
    }

    private IQueryable<Track> FilteredTracks(string? search)
    {
	    var query = db.Tracks.AsNoTracking();

	    return SearchTerm.For(search) is not { Pattern: var pattern } ? query : query.Where(t => EF.Functions.Like(t.Title, pattern, SearchTerm.EscapeChar));
    }

    public async Task<PagedResult<ArtistDto>> GetArtistsAsync(PageRequest page, string? q, CancellationToken ct)
    {
	    var query = db.Artists.AsNoTracking();

	    if (SearchTerm.For(q) is { Pattern: var pattern })
	    {
		    query = query.Where(a => EF.Functions.Like(a.NormalizedName, pattern,  SearchTerm.EscapeChar));
	    }

	    return await query.OrderBy(a => a.NormalizedName).ToPagedAsync(page, ToDto.Artist, ct);
    }

    public async Task<ArtistDetailsDto> GetArtistAsync(Guid id, PageRequest trackPage, CancellationToken ct)
    {
	    var page = trackPage ?? new PageRequest();

	    var artist = await db.Artists.AsNoTracking()
		    .Where(a => a.Id == id)
		    .Select(a => new { a.Id, a.Name, a.ImagePath })
		    .FirstOrDefaultAsync(ct);

	    // var albums =
	    // var tracks =

	    return new ArtistDetailsDto(artist.Id, artist.Name, artist.ImagePath != null);
    }



}
