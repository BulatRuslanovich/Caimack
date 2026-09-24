using Api.Auth;
using App.Abstraction;
using App.Common;
using App.Dtos;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService service, ICurrentUser user, IWebHostEnvironment env) : ControllerBase
{
	private bool IsSecure => AuthCookies.IsSecure(Request, env);


	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<ActionResult<AuthUserDto>> Login(LoginDto dto, CancellationToken ct)
	{
		var res = await service.LoginAsync(dto, ct);
		AuthCookies.Write(Response, res, IsSecure);

		return Ok(res.User);
	}

	[HttpPost("refresh")]
	[AllowAnonymous]
	public async Task<ActionResult<AuthResponseDto>> Refresh(CancellationToken ct)
	{
		try
		{
			var res = await service.RefreshAsync(Request.Cookies[AuthCookies.RTokenCookie], ct);
			AuthCookies.Write(Response, res, IsSecure);

			return Ok(res.User);
		}
		catch (AuthException ex)
		{
			AuthCookies.Clear(Response, IsSecure);
			return Problem(statusCode: 401, title: "Zalupa kakaia to", detail: ex.Message);
		}
	}

	[HttpPost("logout")]
	[AllowAnonymous]
	public async Task<IActionResult> Logout(CancellationToken ct)
	{
		await service.LogoutAsync(Request.Cookies[AuthCookies.RTokenCookie], ct);
		AuthCookies.Clear(Response, IsSecure);

		return NoContent();
	}

	[HttpGet("me")]
	public async Task<ActionResult<AuthUserDto>> Me(CancellationToken ct)
	{
		return Ok(await service.GetMeAsync(user.Id, ct));
	}
}
