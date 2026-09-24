using System.Text.Json;
using App.Dtos;
using Microsoft.AspNetCore.WebUtilities;

namespace Api.Auth;

public static class AuthCookies
{
	public const string ATokenCookie = "access";
	public const string RTokenCookie = "refresh";
	public const string SessionCookie = "session";

	private static readonly JsonSerializerOptions HintJson = new(JsonSerializerDefaults.Web);

	public static bool IsSecure(HttpRequest req, IWebHostEnvironment env)
	{
		return !env.IsDevelopment() || req.IsHttps;
	}

	private static CookieOptions OptionsFor(string path, bool isSecure, DateTimeOffset? expires = null, bool httpOnly = true)
	{
		return new CookieOptions
		{
			HttpOnly = httpOnly,
			Secure = isSecure,
			SameSite = SameSiteMode.Lax,
			Path = path,
			Expires = expires
		};
	}


	public static void Write(HttpResponse resp, AuthResponseDto dto, bool isSecure = false)
	{
		var expires = dto.RTokenExpiresAt;

		resp.Cookies.Append(ATokenCookie, dto.AToken, OptionsFor("/", isSecure,  expires));
		resp.Cookies.Append(RTokenCookie,  dto.RToken,  OptionsFor("/", isSecure,  expires));
		resp.Cookies.Append(SessionCookie,
			WebEncoders.Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(dto.User, HintJson)),
			OptionsFor("/", isSecure,  expires, false));
	}

	public static void Clear(HttpResponse resp, bool isSecure)
	{
		resp.Cookies.Delete(ATokenCookie, OptionsFor("/", isSecure));
		resp.Cookies.Delete(RTokenCookie,  OptionsFor("/", isSecure));
		resp.Cookies.Delete(SessionCookie, OptionsFor("/", isSecure, null, false));
	}
}
