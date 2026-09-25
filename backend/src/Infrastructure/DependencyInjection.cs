using App.Abstraction;
using App.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection s, IConfiguration c)
    {
	    s.AddOptionsFor(c);
        s.AddPersistence(c);
        s.AddAdapters();
    }

    private static void AddPersistence(this IServiceCollection s, IConfiguration c)
    {
        var connectionString = c.GetConnectionString("Main")
        ?? throw new InvalidOperationException("Connection string 'Main' not found.");

        s.AddDbContext<AppDdContext>(options => options.UseNpgsql(connectionString)
	        .UseSnakeCaseNamingConvention(), optionsLifetime: ServiceLifetime.Singleton);

        s.AddDbContextFactory<AppDdContext>(o =>
	        o.UseNpgsql(connectionString).UseSnakeCaseNamingConvention(),
	        lifetime: ServiceLifetime.Singleton);



        s.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDdContext>());
    }

    private static void AddAdapters(this IServiceCollection s)
    {
	    s.AddSingleton<StorageRoot>();
	    s.AddSingleton<IPassHasher, BCryptPassHasher>();
	    s.AddSingleton<ITokenService, JwtTokenService>();
	    s.AddSingleton<IImgStorage, FSImgStorage>();
	    s.AddSingleton<IMusicStorage, FSMusicStorage>();
    }

    private static void AddOptionsFor(this IServiceCollection s, IConfiguration c)
    {
	    JwtOptions.Validator(s.Bind<JwtOptions>(c, JwtOptions.SectionName)).ValidateOnStart();
    }

    private static OptionsBuilder<T> Bind<T>(this IServiceCollection s, IConfiguration c, string section)
	    where T : class
    {
	    return s.AddOptions<T>().Bind(c.GetSection(section));
    }


}
