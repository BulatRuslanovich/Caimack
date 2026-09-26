using App.DTOs;
using App.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumsController(CatalogService service, CoverStreamService cover, ) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<List<AlbumDto>>> List()
	{
		return Ok()
	}
}
