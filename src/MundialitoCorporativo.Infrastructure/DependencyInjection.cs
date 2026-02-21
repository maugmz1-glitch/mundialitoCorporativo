using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Infrastructure.Persistence;

namespace MundialitoCorporativo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b =>
                {
                    b.MigrationsAssembly("MundialitoCorporativo.Infrastructure");
                    b.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                }));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ITeamReadRepository, TeamReadRepository>();
        services.AddScoped<IPlayerReadRepository, PlayerReadRepository>();
        services.AddScoped<IMatchReadRepository, MatchReadRepository>();
        services.AddScoped<IStandingsReadRepository, StandingsReadRepository>();
        services.AddScoped<IRefereeReadRepository, RefereeReadRepository>();
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        return services;
    }
}
