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
