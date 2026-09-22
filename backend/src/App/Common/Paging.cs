using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace App.Common;

public static class Paging
{
	public static async Task<PagedResult<TResult>> ToPagedAsync<TSource, TResult>(
		this IQueryable<TSource> query, PageRequest page, Expression<Func<TSource, TResult>> selector,
		CancellationToken ct
	)
	{
		var total = await query.CountAsync(ct);

		var items = await query
			.Skip(page.Skip)
			.Take(page.PageSize)
			.Select(selector)
			.ToListAsync(ct);

		return new PagedResult<TResult>(items, total, page.Page,  page.PageSize);
	}
}
