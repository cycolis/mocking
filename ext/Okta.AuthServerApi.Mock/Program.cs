using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Okta.AuthServerApi.Mock;
using Okta.AuthServerApi.Mock.Services;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

var host = Host.CreateDefaultBuilder(args)
                .UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production") // Explicit override
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Add appsettings.json and environment-specific appsettings
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();

                    Console.WriteLine($"Current Environment: {environment}");
                })
                .ConfigureServices((context, services) =>
                {
                    // Access configuration values
                    var configuration = context.Configuration;
                    services.Configure<WireMockAppSettings>(configuration.GetSection("WireMockAppSettings"));

                    services.AddHttpClient();

                    // Register application services
                    services.AddSingleton<IMockServerService, MockServerService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

var wireMockService = host.Services.GetRequiredService<IMockServerService>();
Console.WriteLine($"WireMock running at: {wireMockService.Url}");

// Run the application
host.Run();