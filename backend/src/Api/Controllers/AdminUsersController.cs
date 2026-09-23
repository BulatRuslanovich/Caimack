using App.Common;
using App.Dtos;
using App.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class AdminUsersController(AdminUserService service) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<List<User>>> List([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
	{
		return Ok(await service.GetUsersAsync(new PageRequest(page, pageSize), ct));
	}

	[HttpPost]
	public async Task<ActionResult<AuthUserDto>> Create([FromBody] CreateUserDto dto, CancellationToken ct)
	{
		var created = await service.CreateUserAsync(dto, ct);
		return Created($"api/users/{created.Id}", created);
	}

	[HttpPut("{id:guid}/active")]
	public async Task<ActionResult<AuthUserDto>> SetActive([FromRoute] Guid id, SetActiveDto dto, CancellationToken ct)
	{
		return Ok(await service.SetActiveAsync(id,  dto.IsActive, ct));
	}

	[HttpPut("{id:guid}/role")]
	public async Task<ActionResult<AuthUserDto>> SetRole([FromRoute] Guid id, SetRoleDto dto, CancellationToken ct)
	{
		return Ok(await service.SetRoleAsync(id, dto.IsAdmin, ct));
	}

	[HttpPost("{id:guid}/password")]
	public async Task<IActionResult> ResetPassword(Guid id, ResetPassDto dto, CancellationToken ct)
	{
		await service.ResetPassAsync(id,  dto.NewPass ?? string.Empty, ct);
		return NoContent();
	}

	[HttpPut("{id:guid}/revoke")]
	public async Task<IActionResult> RevokeSessions(Guid id, CancellationToken ct)
	{
		await service.RevokeAsync(id, ct);
		return NoContent();
	}

}
