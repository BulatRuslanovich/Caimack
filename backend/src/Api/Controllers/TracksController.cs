using App.Common;
using App.Dtos;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/tracks")]
public class TracksController(CatalogService catalogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TrackDto>>> List([FromQuery] int? page, [FromQuery]int? pageSize, [FromQuery]string? q = null, [FromQuery] CatalogService.TrackSort sort = CatalogService.TrackSort.Title, CancellationToken ct = default) {
        var tracks = await catalogService.GetTracksAsync(new PageRequest(page, pageSize), sort, q, ct);
        return Ok(tracks);
    }
}
