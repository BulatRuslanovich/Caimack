
using App.Common;
using App.DTOs;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/artists")]
public class ArtistsController(CatalogService service, CoverStreamService cover, ArtistProfileService profile) : ControllerBase
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

    [HttpGet("{id:guid}/img")]
    [Produces("image/jpeg", "image/png", "image/webp")]
    public async Task<IActionResult> Image(Guid id, CancellationToken ct, [FromQuery] CoverSize size = CoverSize.Full)
    {
	    return this.ImageFile(await cover.OpenArtistCoverAsync(id, size, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ArtistDto>> Update(Guid id, UpdateArtistDto dto, CancellationToken ct)
    {
	    return Ok(await profile.RenameAsync(id, dto, ct));
    }

    [HttpPost("{id:guid}/img")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ArtistDto>> UpdateImg(Guid id, IFormFile? file, CancellationToken ct)
    {
	    var img = file.RequireImage();
	    await using var stream = img.OpenReadStream();
	    return Ok(await profile.SetImageAsync(id, stream, img.ContentType, img.FileName, img.Length, ct));
    }

    [HttpDelete("{id:guid}/img")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> DeleteImg(Guid id, CancellationToken ct)
    {
	    await profile.RemoveImageAsync(id, ct);
	    return NoContent();
    }
}
