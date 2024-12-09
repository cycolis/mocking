using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockOktaClientLibrary.Services;
using MockOktaClientLibrary.Models;
using Xunit;

public class MockOktaServiceTests
{
    private readonly IServiceProvider _serviceProvider;

    public MockOktaServiceTests()
    {
        var services = new ServiceCollection();

        // Load appsettings for WireMock
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
            .Build();

        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.AddHttpClient();
        services.AddTransient<IMockOktaService, MockOktaService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsExpectedResponse()
    {
        var service = _serviceProvider.GetRequiredService<IMockOktaService>();

        var result = await service.AuthenticateAsync("testuser1@example.com", "password123");

        Assert.Contains("access_token", result);
    }
}