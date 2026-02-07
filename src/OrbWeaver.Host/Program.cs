using OrbWeaver.Application;
using Serilog;
using Hangfire;
using Hangfire.PostgreSql;
using OrbWeaver.Host.Services;
using OrbWeaver.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local"}.json",
            optional: true)
        .Build())
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    builder.Services.AddOpenApi();
    
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddData(connectionString!);
    builder.Services.AddNotifications(builder.Configuration);
    
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => 
            options.UseNpgsqlConnection(connectionString), 
            new PostgreSqlStorageOptions
            {
                SchemaName = "hangfire"
            }))
        ;
    
    builder.Services.AddHangfireServer();
    
    builder
        .Services
        .AddOrbWeaverHandler()
        .AddHostedService<KafkaConsumerService>()
        .AddHostedService<TelegramConsumerService>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    
    app.UseHangfireDashboard();
    
    RecurringJob.AddOrUpdate<OrbWeaver.Application.Jobs.RunAlertsJob>(
        "run-alerts-job",
        job => job.Execute(CancellationToken.None),
        Cron.Minutely);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}