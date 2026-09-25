
using App.Common;
using App.DTOs;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/artists")]
public class ArtistsController(CatalogService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ArtistDto>>> List([FromQuery] int? page, [FromQuery] int? pageSize,  [FromQuery] string? q, CancellationToken ct)
    {
        return Ok(await service.GetArtistsAsync(new PageRequest(page, pageSize), q, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArtistDetailsDto>> GetArtist(Guid id, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
	    return Ok(await service.GetArtistAsync(id, new PageRequest(page, pageSize), ct));
    }


}
