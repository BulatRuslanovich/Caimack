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

	    if (SearchTerm.For(search) is not { Pattern: var pattern }) return query;

	    return query.Where(t => EF.Functions.Like(t.Title, pattern, SearchTerm.EscapeChar));
    }

	

}
