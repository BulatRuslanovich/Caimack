using Microsoft.Extensions.DependencyInjection;
using App.Services;

namespace App;

public static class DependencyInjection
{
    public static IServiceCollection AddApp(this IServiceCollection s)
    {
        s.AddScoped<CatalogService>();
        return s;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection s, IConfiguration config)
    {
        return s;
    }
}