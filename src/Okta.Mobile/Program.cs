using Okta.DeviceAccess.Core.Models;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

var builder = WebApplication.CreateBuilder(args);

// Configure appsettings.json and environment-specific appsettings
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

Console.WriteLine($"Current Environment: {environment}");

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddHttpClient();
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();