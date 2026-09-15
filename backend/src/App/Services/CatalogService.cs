using App.Abstraction;
using App.Common;
using App.Dtos;
using Microsoft.EntityFrameworkCore;

namespace App.Services;

public class CatalogService(IAppDbContext db)
{
    public async Task<PagedResult<TrackDto>> GetTracksAsync(CancellationToken ct)
    {
        var tracks = await db.Tracks
            .Select(t => new TrackDto(t.Id, t.Title))
            .ToListAsync(ct);

        return new PagedResult<TrackDto>(tracks, 0, 1, 50);
    }

}