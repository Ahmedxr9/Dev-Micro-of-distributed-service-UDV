using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Shared.Database;
using System;
using System.Threading.Tasks;

namespace Shared.Application;

/// <summary>
/// Base configuration helper for web applications with common infrastructure setup
/// </summary>
public static class WebApplicationConfiguration
{
    /// <summary>
    /// Configure Serilog with Elasticsearch support
    /// </summary>
    public static void ConfigureSerilog(IConfiguration configuration, string serviceName)
    {
        var elasticsearchUri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console();

        // Add Elasticsearch sink only if URI is valid (won't crash if Elasticsearch is unavailable)
        try
        {
            loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{serviceName.ToLower()}-{{0:yyyy.MM.dd}}",
                FailureCallback = e => Console.WriteLine($"Unable to submit event to Elasticsearch: {e.MessageTemplate}"),
                EmitEventFailure = Serilog.Sinks.Elasticsearch.EmitEventFailureHandling.WriteToSelfLog |
                                  Serilog.Sinks.Elasticsearch.EmitEventFailureHandling.WriteToFailureSink
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not configure Elasticsearch sink: {ex.Message}. Continuing with console logging only.");
        }

        Log.Logger = loggerConfig.CreateLogger();
    }

    /// <summary>
    /// Configure database context with PostgreSQL
    /// </summary>
    public static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=notifications;Username=postgres;Password=postgres;Port=5432";

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<INotificationRepository, NotificationRepository>();
    }

    /// <summary>
    /// Configure health checks for PostgreSQL and RabbitMQ
    /// </summary>
    public static void ConfigureHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=notifications;Username=postgres;Password=postgres;Port=5432";

        var rabbitMQConnectionString = configuration["RabbitMQ:ConnectionString"]
            ?? "amqp://guest:guest@localhost:5672/";

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql")
            .AddRabbitMQ(rabbitConnectionString: rabbitMQConnectionString, name: "rabbitmq");
    }

    /// <summary>
    /// Configure Prometheus metrics
    /// </summary>
    public static void ConfigurePrometheusMetrics(IServiceCollection services, int? metricsPort = null)
    {
        services.UseHttpClientMetrics();
        
        if (metricsPort.HasValue)
        {
            services.AddSingleton<IMetricServer>(sp => new MetricServer(port: metricsPort.Value));
        }
    }

    /// <summary>
    /// Configure common middleware pipeline
    /// </summary>
    public static void ConfigureCommonMiddleware(WebApplication app)
    {
        app.UseSerilogRequestLogging();

        // Prometheus Metrics
        app.UseHttpMetrics();
        app.MapMetrics();

        // Health Checks
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready");
        app.MapHealthChecks("/health/live");
    }

    /// <summary>
    /// Ensure database is created with retry logic
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(WebApplication app, int maxRetries = 5, int delaySeconds = 2)
    {
        await RetryAsync(async () =>
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }
        }, maxRetries, delaySeconds);
    }

    /// <summary>
    /// Retry helper method with exponential backoff
    /// </summary>
    private static async Task RetryAsync(Func<Task> action, int maxRetries = 5, int delaySeconds = 2)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                var waitTime = delaySeconds * (int)Math.Pow(2, i); // Exponential backoff
                Console.WriteLine($"Attempt {i + 1} failed: {ex.Message}. Retrying in {waitTime} seconds...");
                await Task.Delay(TimeSpan.FromSeconds(waitTime));
            }
        }
        throw new InvalidOperationException($"Operation failed after {maxRetries} attempts");
    }
}

