using Microsoft.Extensions.DependencyInjection;

namespace OrbWeaver.Handler;

public static class SetupHandler
{
    public static IServiceCollection AddOrbWeaverHandler(this IServiceCollection services)
        => services.AddSingleton<IUpdateHandler, UpdateHandler>();
}