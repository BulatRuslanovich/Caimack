using App.Abstraction;
using App.Common;
using Microsoft.EntityFrameworkCore;

namespace App.Services;

public record CoverResult(Stream Content, string ContentType, string ETag);

public class CoverStreamService(IAppDbContext db, IMusicStorage music, IImgStorage img, ICurrentUser user)
{
	private CoverResult OpenVariant(string relPath, CoverSize size)
	{
		List<CoverSize> sizes;

		switch (size)
		{
			case CoverSize.Large:
				sizes = [CoverSize.Large, CoverSize.Full, CoverSize.Thumb];
				break;
			case CoverSize.Thumb:
			case CoverSize.Full:
			default:
				sizes = [CoverSize.Full, CoverSize.Thumb];
				break;
		}



		var res = sizes.Select(s => img.CoverVariantPath(relPath, s))
			          .FirstOrDefault(p => music.ResolveExisting(p) is not null)
		          ?? img.CoverVariantPath(relPath, size);

		var fullPath = music.ResolveExisting(res);
		var stream = fullPath is null ? null : music.OpenRead(res);

		if (stream is null) throw new NotFoundException();

		var contentType = Path.GetExtension(res).ToLowerInvariant() switch
		{
			".png" => "image/png",
			".gif" => "image/gif",
			".jpg" or ".jpeg" => "image/jpeg",
			_ => "image/webp"
		};

		var stamp = File.GetLastWriteTimeUtc(fullPath!).Ticks;
		return new CoverResult(stream, contentType, $"\"{stamp:x}-{stream.Length:x}\"");
	}

	public async Task<CoverResult> OpenAlbumCoverAsync(Guid id, CoverSize size, CancellationToken ct)
	{
		var coverPath = await db.Albums.AsNoTracking()
			.Where(a => a.Id == id)
			.Select(a => a.CoverPath)
			.FirstOrDefaultAsync(ct) ?? throw new NotFoundException();

		return OpenVariant(coverPath, size);
	}

	public async Task<CoverResult> OpenArtistCoverAsync(Guid id, CoverSize size, CancellationToken ct)
	{
		var coverPath = await db.Artists.AsNoTracking()
			.Where(a => a.Id == id)
			.Select(a => a.ImagePath)
			.FirstOrDefaultAsync(ct) ?? throw new NotFoundException();

		return OpenVariant(coverPath, size);
	}

	// public async Task<CoverResult> OpenPlaylistCoverAsync(Guid id, CoverSize size, CancellationToken ct)
	// {
	// 	var coverPath = await db.Artists.AsNoTracking()
	// 		.Where(a => a.Id == id)
	// 		.Select(a => a.ImagePath)
	// 		.FirstOrDefaultAsync(ct);
	//
	// 	return string.IsNullOrWhiteSpace(coverPath) ? throw new NotFoundException() : OpenVariant(coverPath, size);
	// }

	// public async Task<CoverResult> OpenTrackCoverAsync(Guid id, CoverSize size, CancellationToken ct)
	// {
	// 	var coverPath = await db.Artists.AsNoTracking()
	// 		.Where(a => a.Id == id)
	// 		.Select(a => a.ImagePath)
	// 		.FirstOrDefaultAsync(ct);
	//
	// 	return string.IsNullOrWhiteSpace(coverPath) ? throw new NotFoundException() : OpenVariant(coverPath, size);
	// }
}
