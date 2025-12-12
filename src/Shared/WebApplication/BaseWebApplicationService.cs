using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Shared.Application;

/// <summary>
/// Base implementation of IWebApplicationService with common infrastructure setup
/// </summary>
public abstract class BaseWebApplicationService : IWebApplicationService
{
    protected abstract string ServiceName { get; }

    /// <summary>
    /// Configure common services (logging, database, health checks, metrics)
    /// </summary>
    public virtual void ConfigureServices(WebApplicationBuilder builder)
    {
        // Configure Serilog
        WebApplicationConfiguration.ConfigureSerilog(builder.Configuration, ServiceName);
        builder.Host.UseSerilog();

        // Configure Database
        WebApplicationConfiguration.ConfigureDatabase(builder.Services, builder.Configuration);

        // Configure Health Checks
        WebApplicationConfiguration.ConfigureHealthChecks(builder.Services, builder.Configuration);

        // Configure Prometheus Metrics (override in derived class if specific port needed)
        WebApplicationConfiguration.ConfigurePrometheusMetrics(builder.Services);

        // Call application-specific service configuration
        ConfigureApplicationServices(builder);
    }

    /// <summary>
    /// Configure common middleware pipeline
    /// </summary>
    public virtual void ConfigurePipeline(WebApplication app)
    {
        // Configure common middleware
        WebApplicationConfiguration.ConfigureCommonMiddleware(app);

        // Call application-specific middleware configuration
        ConfigureApplicationMiddleware(app);

        // Ensure database is created (with retry logic)
        try
        {
            WebApplicationConfiguration.EnsureDatabaseCreatedAsync(app).Wait();
            Log.Information("Database initialized successfully for {ServiceName}", ServiceName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize database after retries for {ServiceName}. Please ensure PostgreSQL is running and accessible.", ServiceName);
            Log.Warning("Application will continue, but database operations may fail until PostgreSQL is available.");
        }
    }

    /// <summary>
    /// Override this method to configure application-specific services
    /// </summary>
    public abstract void ConfigureApplicationServices(WebApplicationBuilder builder);

    /// <summary>
    /// Override this method to configure application-specific middleware
    /// </summary>
    public abstract void ConfigureApplicationMiddleware(WebApplication app);
}

