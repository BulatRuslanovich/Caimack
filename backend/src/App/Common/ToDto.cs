using System.Linq.Expressions;
using App.Dtos;
using Domain.Entities;

namespace App.Common;

public static class ToDto
{
	public static Expression<Func<Track, TrackDto>> Track(Guid userId) => t => new TrackDto(t.Id, t.Title);
}
