namespace App.DTOs;

public record ArtistDto(
    Guid Id,
    string Name,
    int AlbumCount,
    int TrackCount,
    bool HasImg
);

// TODO: когда будут треки и теги, надо будет написать листы для них
public record ArtistDetailsDto(
	Guid Id,
	string Name,
	bool HasImg
);

public record UpdateArtistDto(
    string Name
);

public record AlbumDto(
    Guid Id,
    string Title,
    Guid ArtistId,
    string ArtistName,
    int? Year,
    int TrackCount,
    int DurationSeconds,
    bool HasImg,
    DateTimeOffset CreatedAt);

public record AlbumDetailDto(
	Guid Id,
	string Title,
	Guid ArtistId,
	string ArtistName,
	int? Year,
	bool HasImg,
	int DurationSeconds,
    IReadOnlyList<TrackDto> Tracks
);

public record UpdateAlbumDto(
	string? Title,
	string? ArtistName,
	int? Year
);

public record GenreDto(
    Guid Id,
    string Name,
    int TrackCount,
    IReadOnlyList<Guid> CoverAlbumsIds
);

