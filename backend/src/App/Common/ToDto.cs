using System.Linq.Expressions;
using App.Dtos;
using Domain.Entities;

namespace App.Common;

public static class ToDto
{
	public static Expression<Func<Track, TrackDto>> Track(Guid userId) => t => new TrackDto(t.Id, t.Title);

	public static AuthUserDto FromUser(User u) => new AuthUserDto(u.Id, u.Username, u.IsAdmin, u.IsActive, u.CreatedAt);

	public static Expression<Func<User, AuthUserDto>> AuthUser { get; } = u => FromUser(u);

}
