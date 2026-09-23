using App.Abstraction;
using App.Dtos;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService service) : ControllerBase
{
	public async Task<ActionResult<AuthUserDto>> Login(LoginDto dto, CancellationToken ct)
	{
		var res = await service.LoginAsync(dto, ct);

		return Ok(res.User);
	}
}
