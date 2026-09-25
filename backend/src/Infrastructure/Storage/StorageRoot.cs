namespace Infrastructure.Storage;

public class StorageRoot
{
	public const int BufSize = 64 * 1014;

	public const string MusicDir = "music";
	public const string CoverDir = "cover";
	public const string ArtistDir = "artist";
	public const string PlaylistDir = "playlist";
	public const string TranscodeDir = "transcode";
	public const string HlsDir = "hls";

	private readonly string _root;

	public StorageRoot()
	{
		_root = "/storage";

		foreach (var dir in (string[])[MusicDir, CoverDir, ArtistDir, PlaylistDir, TranscodeDir, HlsDir])
		{
			Directory.CreateDirectory(Path.Combine(_root, dir));
		}
	}

	public string Resolve(string path)
	{
		return Path.GetFullPath(Path.Combine(_root, path));
	}

	public void Delete(string relPath)
	{
		var fullPath = Resolve(relPath);

		try
		{
			if (File.Exists(fullPath)) File.Delete(fullPath);
		}
		catch (Exception _) when (_ is IOException or UnauthorizedAccessException) {}
	}
}
