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
    public async Task<ActionResult<PagedResult<TrackDto>>> List() {
        var tracks = await catalogService.GetTracksAsync(CancellationToken.None);
        return Ok(tracks);
    }
}