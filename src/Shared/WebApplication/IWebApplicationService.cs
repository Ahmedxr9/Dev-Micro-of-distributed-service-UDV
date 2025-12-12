using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shared.Application;

/// <summary>
/// Interface for configuring and building web applications with common infrastructure setup
/// </summary>
public interface IWebApplicationService
{
    /// <summary>
    /// Configure the web application builder with common services
    /// </summary>
    void ConfigureServices(WebApplicationBuilder builder);

    /// <summary>
    /// Configure the web application pipeline
    /// </summary>
    void ConfigurePipeline(WebApplication app);

    /// <summary>
    /// Configure application-specific services (called after common services)
    /// </summary>
    void ConfigureApplicationServices(WebApplicationBuilder builder);

    /// <summary>
    /// Configure application-specific middleware (called after common middleware)
    /// </summary>
    void ConfigureApplicationMiddleware(WebApplication app);
}

