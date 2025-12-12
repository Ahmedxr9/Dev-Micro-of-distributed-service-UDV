using GatewayService;
using Shared.Application;

// Create web application builder
var builder = WebApplication.CreateBuilder(args);

// Create and configure the Gateway service
var gatewayService = new GatewayWebApplicationService();
gatewayService.ConfigureServices(builder);

// Build the application
var app = builder.Build();

// Configure the application pipeline
gatewayService.ConfigurePipeline(app);

// Run the application
app.Run();