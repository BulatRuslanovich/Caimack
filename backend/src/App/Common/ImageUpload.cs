using App.Abstraction;

namespace App.Common;

public static class ImageUpload
{

	private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
	private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

	public static async Task<IReadOnlyList<ResizedImg>> WebpSetAsync(
		IImgProcessor processor,
		Stream content,
		string? contentType,
		string filename,
		long len,
		long maxBytes,
		CancellationToken ct
	)
	{
		if (len > maxBytes) throw new UploadTooLargeException(maxBytes);
		if (contentType is null || !AllowedContentTypes.Contains(contentType.ToLowerInvariant()) || !AllowedExtensions.Contains(Path.GetExtension(filename).ToLowerInvariant()))
			throw new ValidationException();

		using var buf = new MemoryStream();
		await content.CopyToAsync(buf, ct);
		buf.Position = 0;

		return await processor.ToWebpSetAsync(buf, [(int)CoverSize.Large, (int)CoverSize.Full, (int)CoverSize.Thumb],
			ct);

	}
}
