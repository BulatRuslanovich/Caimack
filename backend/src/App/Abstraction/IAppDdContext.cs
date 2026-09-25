using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Abstraction;

public interface IAppDbContext
{
    DbSet<Track> Tracks { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Artist>  Artists { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
