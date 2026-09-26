using App.Abstraction;
using App.Common;
using App.DTOs;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace App.Services;

public class AlbumEditService(IAppDbContext db, IImgStorage img, IImgProcessor processor, TagResolver tag)
{
    public async Task<AlbumDto> UpdateAsync(Guid id, UpdateAlbumDto dto, CancellationToken ct)
    {
        var album = await db.Albums.FirstOrDefaultAsync(a => a.Id == id, ct)
                ?? throw new NotFoundException();

        if (dto.Title is { } title)
        {
            title = title.Trim();

            if (title.Length == 0 || title.Length > 300)
            {
                throw new ValidationException();
            }

            album.Title = title;
            album.NormalizedTitle = Normalize.Key(title);
        }

        if (dto.ArtistName is { } name)
        {
            var artists = await tag.ResolveArtistsAsync([name], ct);
            album.ArtistId = artists[0].Id;
        }

        if (dto.Year is { } year)
        {
            if (year is < 1500 or 2999) throw new ValidationException();
            album.Year = year;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new ConflictException();
        }

        return await db.Albums.AsNoTracking().Where(a => a.Id == id).Select(ToDto.Album).FirstAsync(ct);
    }

    public async Task<AlbumDto> SetCoverAsync(
        Guid id,
        Stream content,
        string? contentType,
        string fileName,
        long len,
        CancellationToken ct
    )
    {
        var album = await db.Albums.FirstOrDefaultAsync(a => a.Id == id, ct)
                ?? throw new NotFoundException();

        var renditions = await ImageUpload.WebpSetAsync(
            processor, content, contentType, fileName, len, 8L * 1024 * 1024, ct);
        
        album.CoverPath = await img.SaveCoverAsync(album.Id, renditions, ct);
        await db.SaveChangesAsync();

        return await db.Albums.AsNoTracking().Where(a => a.Id == id).Select(ToDto.Album).FirstAsync(ct);
    }

    public async Task RemoveCoverAsync(Guid id, CancellationToken ct)
    {
        var album = await db.Albums.FirstOrDefaultAsync(a => a.Id == id, ct)
                ?? throw new NotFoundException();
        
        var path = album.CoverPath;
        if (path is null) return;

        album.CoverPath = null;
        await db.SaveChangesAsync(ct);

        img.DeleteCover(path);
    }
}
