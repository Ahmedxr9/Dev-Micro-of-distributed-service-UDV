using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using GatewayService.Validators;
using GatewayService.Mapping;
using Shared.Messaging;
using Shared.Application;
using FluentValidation;
using Prometheus;

namespace GatewayService;

/// <summary>
/// Gateway Service web application configuration
/// </summary>
public class GatewayWebApplicationService : BaseWebApplicationService
{
    protected override string ServiceName => "gateway-service";

    /// <summary>
    /// Configure Gateway-specific services
    /// </summary>
    public override void ConfigureApplicationServices(WebApplicationBuilder builder)
    {
        // Controllers
        builder.Services.AddControllers();

        // Swagger/OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Notification Gateway API",
                Version = "v1",
                Description = "Distributed Microservice-Based Notification System Gateway"
            });
        });

        // Message Producer
        builder.Services.AddSingleton<IMessageProducer>(sp =>
        {
            var connectionString = builder.Configuration["RabbitMQ:ConnectionString"]
                ?? "amqp://guest:guest@localhost:5672/";
            var logger = sp.GetService<ILogger<RabbitMQProducer>>();
            return new RabbitMQProducer(connectionString, logger);
        });

        // AutoMapper
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        // FluentValidation
        builder.Services.AddValidatorsFromAssemblyContaining<NotificationRequestValidator>();
    }

    /// <summary>
    /// Configure Gateway-specific middleware
    /// </summary>
    public override void ConfigureApplicationMiddleware(WebApplication app)
    {
        // Swagger UI - Enable in all environments for easier testing
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Gateway API v1");
            c.RoutePrefix = "swagger"; // Set Swagger UI at the root of /swagger
        });

        app.UseRouting();
        app.UseAuthorization();
        app.MapControllers();
    }
}

