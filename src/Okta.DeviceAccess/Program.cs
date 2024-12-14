using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Okta.DeviceAccess.Core.Interfaces;
using Okta.DeviceAccess.Core.Models;
using Okta.DeviceAccess.Core.Services;

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
        services.AddTransient<IOktaService, OktaService>();
    })
    .Build();

using var scope = host.Services.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IOktaService>();

// Test the service
var result = await service.AuthenticateAsync("testuser1@example.com", "password123");
Console.WriteLine(result);