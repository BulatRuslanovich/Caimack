using App.Abstraction;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDdContext(DbContextOptions<AppDdContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Track> Tracks => Set<Track>();
}