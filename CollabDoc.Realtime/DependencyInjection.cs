using Microsoft.Extensions.DependencyInjection;

namespace CollabDoc.Realtime;

public static class DependencyInjection
{
    public static IServiceCollection AddRealtime(this IServiceCollection services)
    {
        services.AddSignalR();

        return services;
    }
}
