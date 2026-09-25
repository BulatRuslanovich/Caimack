using App.Abstraction;
using App.Common;

namespace Infrastructure.Storage;

public class FSImgStorage(StorageRoot root) : IImgStorage
{
	public Task<string> SaveCoverAsync(Guid albumId, IReadOnlyList<ResizedImg> renditions, CancellationToken ct)
	{
		return SaveRenditionsAsync($"{StorageRoot.CoverDir}/{albumId:N}.webp", renditions, ct);
	}

	public Task<string> SaveArtistImgAsync(Guid artistId, IReadOnlyList<ResizedImg> renditions, CancellationToken ct)
	{
		return SaveRenditionsAsync($"{StorageRoot.ArtistDir}/{artistId:N}.webp", renditions, ct);
	}

	public Task<string> SavePlaylistImgAsync(Guid playlistId, IReadOnlyList<ResizedImg> renditions, CancellationToken ct)
	{
		return SaveRenditionsAsync($"{StorageRoot.PlaylistDir}/{playlistId:N}.webp", renditions, ct);
	}

	public string CoverVariantPath(string coverPath, CoverSize size)
	{
		if (size == CoverSize.Full || string.IsNullOrWhiteSpace(coverPath))
		{
			return coverPath;
		}

		var suffix = size == CoverSize.Large ? ".large.webp" : ".thumb.webp";
		return Path.ChangeExtension(coverPath, null) + suffix;
	}

	public void DeleteCover(string path)
	{
		root.Delete(path);
		root.Delete(CoverVariantPath(path, CoverSize.Thumb));
		root.Delete(CoverVariantPath(path, CoverSize.Large));
	}

	private async Task<string> SaveRenditionsAsync(string fullPath, IReadOnlyList<ResizedImg> renditions,
		CancellationToken ct)
	{
		var baseEdge = renditions
			.Select(r => r.Edge)
			.Where(e => e != (int)CoverSize.Large)
			.DefaultIfEmpty(renditions.Max(r => r.Edge))
			.Max();

		foreach (var r in renditions)
		{
			var rel = r.Edge == baseEdge ? fullPath : CoverVariantPath(fullPath, r.Edge == (int)CoverSize.Large ? CoverSize.Large : CoverSize.Thumb);

			var absPath = root.Resolve(rel);
			Directory.CreateDirectory(Path.GetDirectoryName(absPath)!);
			await File.WriteAllBytesAsync(absPath, r.Content, ct);
		}

		return fullPath;
	}



}
