using System.Linq.Expressions;
using App.DTOs;
using Domain.Entities;

namespace App.Common;

public static class ToDto
{
	public static Expression<Func<Track, TrackDto>> Track(Guid userId) => t => new TrackDto(t.Id, t.Title);

	public static AuthUserDto FromUserToAuth(User u) => new(u.Id, u.Username, u.IsAdmin, u.IsActive, u.CreatedAt);

	public static Expression<Func<User, AuthUserDto>> AuthUser { get; } = u => FromUserToAuth(u);

	public static UserDto FromUser(User u) => new UserDto(u.Id, u.Username, u.IsAdmin);

	public static Expression<Func<User, UserDto>> User { get; } = u => FromUser(u);

	//TODO: когда будет альбомы и треки надо заменить нули
	public static Expression<Func<Artist, ArtistDto>> Artist { get; } = a => new ArtistDto(
		a.Id, a.Name, 0, 0, a.ImagePath != null);

	public static Expression<Func<Album, AlbumDto>> Album => a => new AlbumDto (
		a.Id,
		a.Title,
		a.ArtistId,
		a.Artist!.Name,
		a.Year,
		0,
		0,
		a.CoverPath != null,
		a.CreatedAt
	);

	public static Expression<Func<Genre, GenreDto>> Genre => g => new GenreDto(
		g.Id, g.Name, 0, Array.Empty<Guid>()
	);

}
