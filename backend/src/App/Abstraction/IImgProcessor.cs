namespace App.Abstraction;

public record ResizedImg(int Edge, byte[] Content);

public interface IImgProcessor
{
	Task<IReadOnlyList<ResizedImg>> ToWebpSetAsync(Stream src, IReadOnlyList<int> edges, CancellationToken ct);
}

