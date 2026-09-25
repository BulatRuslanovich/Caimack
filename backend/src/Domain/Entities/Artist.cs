
namespace Domain.Entities;


public class Artist
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string? ImagePath { get; set; }

    public DateTimeOffset TagsFetchedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    

}