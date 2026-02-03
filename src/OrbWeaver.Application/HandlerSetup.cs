using Microsoft.Extensions.DependencyInjection;
using OrbWeaver.Application.Abstractions;
using OrbWeaver.Application.Handler;
using OrbWeaver.Application.Jobs;

namespace OrbWeaver.Application;

public static class HandlerSetup
{
    public static IServiceCollection AddOrbWeaverHandler(this IServiceCollection services)
        => services
            .AddSingleton<IUpdateHandler, UpdateHandler>()
            .AddTransient<RunAlertsJob>();
}