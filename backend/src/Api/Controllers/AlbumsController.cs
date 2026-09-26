using App.Common;
using App.DTOs;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumsController(CatalogService service, CoverStreamService cover, AlbumEditService edit) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<List<AlbumDto>>> List(
		[FromQuery] int? page,
		[FromQuery] int? pageSize,
        [FromQuery] Guid? artistId,
        [FromQuery] string? q = null,
        [FromQuery] bool filterByRecent = false,
        CancellationToken ct = default
	)
	{
		return Ok(await service.GetAlbumsAsync(new PageRequest(page, pageSize), artistId, filterByRecent, q, ct));
	}


	[HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
	public async Task<ActionResult<AlbumDto>> Update(Guid id, UpdateAlbumDto dto, CancellationToken ct)
	{
		return Ok(await edit.UpdateAsync(id, dto, ct));
	}

	[HttpGet("{id:guid}/cover")]
    [Produces("image/webp", "image/jpeg", "image/png")]
	public async Task<IActionResult> Cover(
        Guid id, CancellationToken ct, [FromQuery] CoverSize size = CoverSize.Full)
	{
		return this.ImageFile(await cover.OpenAlbumCoverAsync(id, size, ct));
	}

	[HttpPost("{id:guid}/cover")]
    [Authorize(Policy = "Admin")]
	public async Task<ActionResult<AlbumDto>> UploadCover(Guid id, IFormFile? file, CancellationToken ct)
	{
		var img = file.RequireImage();

		await using var stream = img.OpenReadStream();
		return Ok(await edit.SetCoverAsync(id, stream, img.ContentType, img.FileName, img.Length, ct));
	}

	[HttpDelete("{id:guid}/cover")]
    [Authorize(Policy = "Admin")]
	public async Task<IActionResult> DeleteCover(Guid id, CancellationToken ct)
    {
        await edit.RemoveCoverAsync(id, ct);
        return NoContent();
    }
}
