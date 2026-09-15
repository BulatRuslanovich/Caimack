using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection s, IConfiguration c)
    {
        s.AddPersistence(c);
        return s;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection s, IConfiguration c)
    {
        var connectionString = c.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        s.AddDbContext<AppDdContext>(options => options.UseNpgsql(connectionString));
        return s;
    }
}