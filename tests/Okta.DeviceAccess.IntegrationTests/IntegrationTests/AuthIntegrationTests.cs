using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

using Okta.DeviceAccess.Core.Interfaces;
using Okta.DeviceAccess.Core.Services;
using Okta.DeviceAccess.Core.Models;

public class AuthIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;

    public AuthIntegrationTests()
    {
        var services = new ServiceCollection();

        // Load appsettings for WireMock
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
            .Build();

        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.AddHttpClient();
        services.AddTransient<IOktaService, OktaService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsSuccess_ValidCredentials()
    {
        var service = _serviceProvider.GetRequiredService<IOktaService>();

        var result = await service.AuthenticateAsync("testuser1@example.com", "password123");

        Assert.Contains("id_token", result);
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsError_InvalidCredentials()
    {
        var service = _serviceProvider.GetRequiredService<IOktaService>();

        var result = await service.AuthenticateAsync("testuser1@example.com", "wrong");

        Assert.Contains("401", result);
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsError_MobileClientOff()
    {
        var service = _serviceProvider.GetRequiredService<IOktaService>();

        var result = await service.AuthenticateAsync("testuser1@example.com", "password123");

        Assert.Contains("primary-authenticate", result);
    }
}