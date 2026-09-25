using App.Common;
using App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Api;

public static class MediaResults
{
	public static IFormFile RequireImage(this IFormFile? file)
	{
		if (file is not null && file.Length > 0) return file;
		throw new ValidationException();
	}

	public static IActionResult ImageFile(this ControllerBase clr, CoverResult img)
	{
		clr.Response.Headers.CacheControl = "private, max-age=2592000, stale-while-revalidate=31536000";

		return new FileStreamResult(img.Content, img.ContentType)
		{
			EntityTag = EntityTagHeaderValue.Parse(img.ETag)
		};
	}


}
