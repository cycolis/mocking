using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MockOktaClientLibrary.Services;
using MockOktaClientLibrary.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // Bind the configuration section
        services.Configure<AppSettings>(context.Configuration.GetSection("ApiSettings"));
        services.AddHttpClient();
        services.AddTransient<IMockOktaService, MockOktaService>();
    })
    .Build();

using var scope = host.Services.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IMockOktaService>();

// Test the service
var result = await service.AuthenticateAsync("testuser1@example.com", "password123");
Console.WriteLine(result);