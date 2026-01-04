using Microsoft.Extensions.DependencyInjection;

namespace OrbWeaver.Handler;

public static class HandlerSetup
{
    public static IServiceCollection AddOrbWeaverHandler(this IServiceCollection services)
        => services.AddSingleton<IUpdateHandler, UpdateHandler>();
}