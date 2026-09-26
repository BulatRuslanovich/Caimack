using App.Abstraction;
using App.Common;
using App.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Services;

public class CatalogService(IAppDbContext db, ICurrentUser user)
{
	public enum TrackSort { Title, Recent, Artist, Album }

    public async Task<PagedResult<TrackDto>> GetTracksAsync(PageRequest page, TrackSort sort, string? search, CancellationToken ct)
    {
	    var query = FilteredTracks(search);

	    var ordered = sort switch
	    {
		    TrackSort.Recent => query.OrderByDescending(t => t.Id),
		    _ => query.OrderBy(t => t.Title)
	    };

        return await ordered.ToPagedAsync(page, ToDto.Track(Guid.CreateVersion7()), ct);
    }

    private IQueryable<Track> FilteredTracks(string? search)
    {
	    var query = db.Tracks.AsNoTracking();

	    return SearchTerm.For(search) is not { Pattern: var pattern } ? query : query.Where(t => EF.Functions.Like(t.Title, pattern, SearchTerm.EscapeChar));
    }

    public async Task<PagedResult<ArtistDto>> GetArtistsAsync(PageRequest page, string? q, CancellationToken ct)
    {
	    var query = db.Artists.AsNoTracking();

	    if (SearchTerm.For(q) is { Pattern: var pattern })
	    {
		    query = query.Where(a => EF.Functions.Like(a.NormalizedName, pattern,  SearchTerm.EscapeChar));
	    }

	    return await query.OrderBy(a => a.NormalizedName).ToPagedAsync(page, ToDto.Artist, ct);
    }

    public async Task<ArtistDetailsDto> GetArtistAsync(Guid id, PageRequest trackPage, CancellationToken ct)
    {
	    var page = trackPage ?? new PageRequest();

	    var artist = await db.Artists.AsNoTracking()
		    .Where(a => a.Id == id)
		    .Select(a => new { a.Id, a.Name, a.ImagePath })
		    .FirstOrDefaultAsync(ct);

	    // var albums =
	    // var tracks =

	    return new ArtistDetailsDto(artist.Id, artist.Name, artist.ImagePath != null);
    }

	public async Task<PagedResult<AlbumDto>> GetAlbumsAsync(
        PageRequest page,
        Guid? artistId,
        bool filterByRecent,
        string? q,
        CancellationToken ct)
    {
        var query = db.Albums.AsNoTracking();

        if (artistId is not null)
            query = query.Where(a => a.ArtistId == artistId);

        if (SearchTerm.For(q) is { Pattern: var pattern })
        {
            query = query.Where(a =>
                EF.Functions.Like(a.NormalizedTitle, pattern, SearchTerm.EscapeChar)
                || EF.Functions.Like(a.Artist!.NormalizedName, pattern, SearchTerm.EscapeChar));
        }

        var ordered = filterByRecent
            ? query.OrderByDescending(a => a.CreatedAt)
            : query.OrderBy(a => a.Title);

        return await ordered.ToPagedAsync(page, ToDto.Album, ct);
    }

	// public async Task<AlbumDetailDto> GetAlbumAsync(Guid id, CancellationToken ct)
    // {
    //     var album = await db.Albums.AsNoTracking()
    //         .Where(a => a.Id == id)
    //         .Select(a => new
    //         {
    //             a.Id,
    //             a.Title,
    //             a.ArtistId,
    //             ArtistName = a.Artist!.Name,
    //             a.Year,
    //             HasCover = a.CoverPath != null,
    //             Duration = a.Tracks.Sum(t => t.DurationSeconds),
    //         })
    //         .FirstOrDefaultAsync(ct)
    //         ?? throw new NotFoundException();

    //     var tracks = await db.Tracks.AsNoTracking()
    //         .Where(t => t.AlbumId == id)
    //         .OrderBy(t => t.DiscNumber ?? 1)
    //         .ThenBy(t => t.TrackNumber ?? int.MaxValue)
    //         .ThenBy(t => t.Title)
    //         .Select(ToDto.Track(user.Id))
    //         .ToListAsync(ct);

    //     return new AlbumDetailDto(
    //         album.Id, album.Title, album.ArtistId, album.ArtistName,
    //         album.Year, album.HasCover, album.Duration, tracks);
    // }
}
