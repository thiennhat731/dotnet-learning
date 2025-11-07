using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CollabDoc.Application.Interfaces;
using CollabDoc.Infrastructure.Repositories;
using CollabDoc.Infrastructure.Security;
using CollabDoc.Infrastructure.Settings;
using CollabDoc.Infrastructure.Mappings;
using CollabDoc.Infrastructure.Services;

namespace CollabDoc.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ✅ OPTIONS PATTERN chuẩn
        services.AddOptionsConfiguration(configuration);

        // ✅ Cache Service
        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();

        // ✅ Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // ✅ Security
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }

    private static IServiceCollection AddOptionsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<MongoDbSettings>()
            .Bind(configuration.GetSection(MongoDbSettings.SectionName))
            .ValidateOnStart();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateOnStart();

        services.AddOptions<CorsSettings>()
            .Bind(configuration.GetSection(CorsSettings.SectionName))
            .ValidateOnStart();

        return services;
    }

    public static void InitializeMongoMaps()
    {
        DocumentMap.RegisterClassMap();
        UserMap.RegisterClassMap();
    }
}
