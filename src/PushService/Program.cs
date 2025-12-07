using Microsoft.EntityFrameworkCore;
using Shared.Database;
using Shared.Messaging;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using PushService.Services;
using PushService.Providers;
using Prometheus;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Logging (Serilog Only)
// =======================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
        new Uri(builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "push-service-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

builder.Host.UseSerilog();

// =======================
// Database (PostgreSQL)
// =======================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=notifications;Username=postgres;Password=postgres;Port=5432";

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// =======================
// RabbitMQ Consumer
// =======================
builder.Services.AddSingleton<IMessageConsumer>(sp =>
{
    var mqConnection = builder.Configuration["RabbitMQ:ConnectionString"]
        ?? "amqp://guest:guest@localhost:5672/";

    var logger = sp.GetRequiredService<ILogger<RabbitMQConsumer>>();
    return new RabbitMQConsumer(mqConnection, logger);
});

// =======================
// Push Provider (Mock)
// =======================
builder.Services.AddSingleton<IPushProvider, MockPushProvider>();

// =======================
// Push Service + Worker
// =======================
builder.Services.AddScoped<IPushService, PushService.Services.PushService>();
builder.Services.AddHostedService<PushWorker>();

// =======================
// Health Checks (Basic)
// =======================
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddRabbitMQ(
        rabbitConnectionString: builder.Configuration["RabbitMQ:ConnectionString"] 
            ?? "amqp://guest:guest@localhost:5672/",
        name: "rabbitmq");

// =======================
// Prometheus Metrics
// =======================
builder.Services.AddSingleton<IMetricServer>(sp => new MetricServer(port: 9093));

// =======================
// Build App
// =======================
var app = builder.Build();

app.UseMetricServer();
app.MapMetrics();

// BASIC HEALTH CHECKS (NO UI)
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// =======================
// Auto-create database
// =======================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();