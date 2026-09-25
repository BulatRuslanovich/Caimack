using App.Abstraction;
using App.Common;
using App.DTOs;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace App.Services;

public class ArtistProfileService(IAppDbContext db, IImgProcessor processor, IImgStorage img)
{
	public async Task<ArtistDto> RenameAsync(Guid id, UpdateArtistDto dto, CancellationToken ct)
	{
		var artist = await db.Artists.FirstOrDefaultAsync(a => a.Id == id, ct) ?? throw new NotFoundException();
		var name = dto.Name.Trim();

		if (name.Length > 200) throw new ValidationException();

		var key = Normalize.Key(name);
		artist.Name = name;
		artist.NormalizedName = key;

		try
		{
			await db.SaveChangesAsync(ct);
		}
		catch (DbUpdateException)
		{
			throw new ConflictException();
		}

		return await db.Artists.AsNoTracking().Where(a => a.Id == id).Select(ToDto.Artist).FirstAsync(ct);
	}

	public async Task<ArtistDto> SetImageAsync(Guid id, Stream content, string? contentType, string fileName, long len, CancellationToken ct)
	{
		var artist = await db.Artists.FirstOrDefaultAsync(a => a.Id == id, ct) ?? throw new NotFoundException();

		var renditions =
			await ImageUpload.WebpSetAsync(processor, content, contentType, fileName, len, 8L * 1024 ^ 2, ct);

		artist.ImagePath = await img.SaveArtistImgAsync(artist.Id, renditions, ct);
		await db.SaveChangesAsync(ct);

		return await db.Artists.AsNoTracking().Where(a => a.Id == id).Select(ToDto.Artist).FirstAsync(ct);
	}

	public async Task RemoveImageAsync(Guid id, CancellationToken ct)
	{
		var artist = await db.Artists.FirstOrDefaultAsync(a => a.Id == id, ct) ?? throw new NotFoundException();
		var path = artist.ImagePath;

		if (path is null) return;

		artist.ImagePath = null;
		await db.SaveChangesAsync(ct);
		img.DeleteCover(path);
	}
}
