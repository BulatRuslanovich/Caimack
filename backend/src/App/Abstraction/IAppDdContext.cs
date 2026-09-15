using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Abstraction;

public interface IAppDbContext
{
    DbSet<Track> Tracks { get; }
}