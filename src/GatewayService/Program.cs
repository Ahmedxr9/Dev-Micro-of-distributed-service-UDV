using Microsoft.EntityFrameworkCore;
using Shared.Database;
using Shared.Messaging;
using FluentValidation;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Reflection;
using GatewayService.Validators;
using GatewayService.Mapping;
using Prometheus;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "notification-gateway-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
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

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=notifications;Username=postgres;Password=postgres";
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

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

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddRabbitMQ(
        rabbitConnectionString: builder.Configuration["RabbitMQ:ConnectionString"] ?? "amqp://guest:guest@localhost:5672/",
        name: "rabbitmq");

builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(10);
    setup.MaximumHistoryEntriesPerEndpoint(50);
}).AddInMemoryStorage();

// Prometheus Metrics
builder.Services.AddSingleton<IMetricServer>(sp => new MetricServer(port: 9090));
builder.Services.UseHttpClientMetrics();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

// Prometheus Metrics
app.UseMetricServer();
app.UseHttpMetrics();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

// Health Checks
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();