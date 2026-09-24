using System.Linq.Expressions;
using App.Dtos;
using Domain.Entities;

namespace App.Common;

public static class ToDto
{
	public static Expression<Func<Track, TrackDto>> Track(Guid userId) => t => new TrackDto(t.Id, t.Title);

	public static AuthUserDto FromUserToAuth(User u) => new(u.Id, u.Username, u.IsAdmin, u.IsActive, u.CreatedAt);

	public static Expression<Func<User, AuthUserDto>> AuthUser { get; } = u => FromUserToAuth(u);

	public static UserDto FromUser(User u) => new UserDto(u.Id, u.Username, u.IsAdmin);

	public static Expression<Func<User, UserDto>> User { get; } = u => FromUser(u);

}
