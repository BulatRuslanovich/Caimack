using App.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection s, IConfiguration c)
    {
        s.AddPersistence(c);
        s.AddAdapters();
        return s;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection s, IConfiguration c)
    {
        var connectionString = c.GetConnectionString("Main")
        ?? throw new InvalidOperationException("Connection string 'Main' not found.");

        s.AddDbContext<AppDdContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        s.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDdContext>());
        return s;
    }

    private static void AddAdapters(this IServiceCollection s)
    {
	    s.AddSingleton<IPassHasher, BCryptPassHasher>();
    }

}
