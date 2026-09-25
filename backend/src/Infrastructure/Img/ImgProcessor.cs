using System.ComponentModel.DataAnnotations;
using App.Abstraction;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Infrastructure.Img;

public class ImgProcessor : IImgProcessor
{
	public async Task<IReadOnlyList<ResizedImg>> ToWebpSetAsync(Stream src, IReadOnlyList<int> edges, CancellationToken ct)
	{
		if (edges.Count == 0) throw new ArgumentException();

		try
		{
			var info = await Image.IdentifyAsync(src, ct);
			if ((long)info.Width * info.Height > 12_000_000) throw new ValidationException();

			src.Position = 0;
			using var image = await Image.LoadAsync(new DecoderOptions { MaxFrames = 1 }, src, ct);

			image.Mutate(c => c.AutoOrient());
			var descending = edges.Distinct().OrderByDescending(e => e).ToList();
			var fitting = descending.Where(e => e <= Math.Min(info.Width, info.Height)).ToList();
			var wanted = fitting.Count > 0 ? fitting : [descending[^1]];
			var rendered = new List<ResizedImg>(wanted.Count);
			foreach (var edge in wanted)
			{
				image.Mutate(c => c.Resize(new ResizeOptions { Mode = ResizeMode.Crop, Size = new Size(edge, edge), Position = AnchorPositionMode.Center, Sampler = LanczosResampler.Lanczos3}));
				using var output = new MemoryStream();

				await image.SaveAsWebpAsync(output, new WebpEncoder { Quality = 82 }, ct);
				rendered.Add(new ResizedImg(edge, output.ToArray()));
			}

			return rendered;
		}
		catch (Exception e) when (e is UnknownImageFormatException or InvalidImageContentException)
		{
			throw new ValidationException();
		}
	}
}
