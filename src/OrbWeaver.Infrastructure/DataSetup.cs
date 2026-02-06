using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrbWeaver.Application.Abstractions;
using OrbWeaver.Infrastructure.Repositories;

namespace OrbWeaver.Infrastructure;

public static class DataSetup
{
    public static IServiceCollection AddData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextFactory<OrbWeaverDbContext>(options =>
        {
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        });

        return
            services.AddDbContext<OrbWeaverDbContext>(options => { options.UseNpgsql(connectionString); })
                .AddScoped<IMessageLogRepository, MessageLogRepository>()
                .AddScoped<IAlertsRepository, AlertsRepository>();
    }
}