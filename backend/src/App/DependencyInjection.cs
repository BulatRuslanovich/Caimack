using Microsoft.Extensions.DependencyInjection;
using App.Services;
using Microsoft.Extensions.Configuration;

namespace App;

public static class DependencyInjection
{
    public static IServiceCollection AddApp(this IServiceCollection s)
    {
        s.AddScoped<CatalogService>();
        return s;
    }
}
