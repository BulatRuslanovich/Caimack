using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDdContext(DbContextOptions<AppDdContext> options) : DbContext(options)
{
    public DbSet<Track> Tracks => Set<Track>();
}