using day11.Application.Interfaces;
using day11.Infrastructure.Repositories;
using day11.Infrastructure.Services;
using day11.Infrastructure.Jwt;
using Microsoft.Extensions.DependencyInjection;

namespace day11.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Repository
        services.AddScoped<IUserRepository, UserRepository>();

        // JWT + Auth services
        services.AddSingleton<JwtSettings>();
        services.AddSingleton<IJwtService, JwtService>();

        // External API (nếu có)
        // services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        return services;
    }
}
