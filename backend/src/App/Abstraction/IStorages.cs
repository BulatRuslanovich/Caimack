using App.Common;

namespace App.Abstraction;

public record StoredFile(string RelativePath, long Size, string Hash);

public interface IImgStorage
{
	Task<string> SaveCoverAsync(Guid albumId, IReadOnlyList<ResizedImg> renditions, CancellationToken ct);
	Task<string> SaveArtistImgAsync(Guid artistId, IReadOnlyList<ResizedImg> renditions, CancellationToken ct);
	Task<string> SavePlaylistImgAsync(Guid playlistId, IReadOnlyList<ResizedImg> renditions, CancellationToken ct);
	string CoverVariantPath(string coverPath, CoverSize size);
	void DeleteCover(string path);
}

public interface IMusicStorage
{
	Task<StoredFile> SaveTrackAsync(Stream content, string ext, long maxBytes, CancellationToken ct);
	public Stream? OpenRead(string relPath);
	string? ResolveExisting(string relPath);
	string ResolveForWrite(string relPath);
	void Delete(string relPath);
}
