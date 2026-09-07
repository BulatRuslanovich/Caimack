using App.Common;
using App.Dtos;

namespace App.Services;

public class CatalogService()
{
    public async Task<PagedResult<TrackDto>> GetTracksAsync(CancellationToken ct)
    {
        return new PagedResult<TrackDto>(new List<TrackDto>(), 0, 1, 50);
    }
}