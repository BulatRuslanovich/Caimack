using System.Security.Cryptography;
using App.Abstraction;
using App.Common;
using Domain.Entities;

namespace Infrastructure.Storage;

public class FSMusicStorage(StorageRoot root) : IMusicStorage
{
	public async Task<StoredFile> SaveTrackAsync(Stream content, string ext, long maxBytes, CancellationToken ct)
	{
		var id = Guid.CreateVersion7().ToString("N");

		var relativePath = $"{StorageRoot.MusicDir}/{id[^2]}/{id[^4..^2]}/{id}{ext}";
		var fullPath = root.Resolve(relativePath);

		Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

		long size = 0;
		byte[] hash;

		try
		{
			await using var target = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: StorageRoot.BufSize, useAsync: true);

			using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

			var buffer = new byte[StorageRoot.BufSize];

			while (true)
			{
				var read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);
				if (read == 0) break;

				size += read;
				if (size > maxBytes) throw new UploadTooLargeException(maxBytes);

				hasher.AppendData(buffer, 0, read);
				await target.WriteAsync(buffer.AsMemory(0, read), ct);
			}

			await target.FlushAsync(ct);
			hash = hasher.GetHashAndReset();
		}
		catch
		{
			if (File.Exists(fullPath)) File.Delete(fullPath);
			throw;
		}

		return new StoredFile(relativePath, size, Convert.ToHexString(hash).ToLowerInvariant());
	}

	public Stream? OpenRead(string relPath)
	{
		var absPath = root.Resolve(relPath);

		if (!File.Exists(absPath)) return null;

		return new FileStream(absPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: StorageRoot.BufSize,
			FileOptions.Asynchronous | FileOptions.SequentialScan);
	}

	public string? ResolveExisting(string relPath)
	{
		var absPath = root.Resolve(relPath);
		return File.Exists(absPath) ? absPath : null;
	}

	public string ResolveForWrite(string relPath)
	{
		var absPath = root.Resolve(relPath);
		Directory.CreateDirectory(Path.GetDirectoryName(absPath)!);
		return absPath;
	}

	public void Delete(string relPath)
	{
		root.Delete(relPath);
	}
}
