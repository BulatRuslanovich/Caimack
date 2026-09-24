using Microsoft.Extensions.DependencyInjection;
using App.Services;

namespace App;

public static class DependencyInjection
{
    public static void AddApp(this IServiceCollection s)
    {
	    s.AddSingleton(TimeProvider.System);

        s.AddScoped<CatalogService>();
        s.AddScoped<AdminUserService>();
        s.AddScoped<AuthService>();

        s.AddSingleton<LoginAttemptTracker>();
    }
}
