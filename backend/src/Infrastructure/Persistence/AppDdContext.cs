using App.Abstraction;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDdContext(DbContextOptions<AppDdContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken>  RefreshTokens => Set<RefreshToken>();
    public DbSet<Artist> Artists => Set<Artist>();
}
