using Microsoft.EntityFrameworkCore;
using Shared.Database;
using Shared.Messaging;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using EmailService.Services;
using EmailService.Providers;
using Prometheus;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
        IndexFormat = "email-service-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=notifications;Username=postgres;Password=postgres;Port=5432";
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Message Consumer
builder.Services.AddSingleton<IMessageConsumer>(sp =>
{
    var connectionString = builder.Configuration["RabbitMQ:ConnectionString"] 
        ?? "amqp://guest:guest@localhost:5672/";
    var logger = sp.GetService<ILogger<RabbitMQConsumer>>();
    return new RabbitMQConsumer(connectionString, logger);
});

// Email Provider (Mock SMTP)
builder.Services.AddSingleton<IEmailProvider, MockEmailProvider>();

// Email Service
builder.Services.AddScoped<IEmailService, EmailService.Services.EmailService>();

// Worker
builder.Services.AddHostedService<EmailWorker>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddRabbitMQ(
        rabbitConnectionString: builder.Configuration["RabbitMQ:ConnectionString"] ?? "amqp://guest:guest@localhost:5672/",
        name: "rabbitmq");

// Prometheus Metrics
builder.Services.AddSingleton<IMetricServer>(sp => new MetricServer(port: 9091));

var app = builder.Build();

// Prometheus Metrics
app.UseMetricServer();
app.MapMetrics();

// Health Checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();

