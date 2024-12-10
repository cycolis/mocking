using WireMock.Matchers;
using WireMock.Server;
using WireMock.Settings;

namespace MockOktaWireMockServer;

public static class MockServerConfig
{
    public static WireMockServer CreateMockServer()
    {
        var server = WireMockServer.Start(new WireMockServerSettings
        {
            Urls = new[] { "http://localhost:9090" },
            StartAdminInterface = true,
            ReadStaticMappings = true
        });

        const string validationScenario = "ValidationFlow";

        // Mock for token endpoint
        server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/oauth2/v1/token")
                .UsingPost())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new
                {
                    access_token = "1eyJhb[...]56Rg",
                    expires_in = 3600,
                    id_token = "eyJhb[...]yosFQ",
                    scope = "openid",
                    token_type = "Bearer"
                }));

        // Mock for OOB request
        server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/oauth2/v1/primary-authenticate")
                .UsingPost())
            .InScenario(validationScenario)
            .WhenStateIs("Started")
            .WillSetStateTo("waiting_for_validation")
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new
                {
                    oob_code = "ftpvP1LB26vCARL7EWM66cUhPA2vdQmHFp",
                    expires_in = 300,
                    interval = 5,
                    channel = "push",
                    binding_method = "transfer",
                    binding_code = "95"
                }));

        // Mock for validate endpoint
        server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/oauth2/v1/mfa/validate")
                .UsingPost())
            .InScenario(validationScenario)
            .WhenStateIs("waiting_for_validation")
            .WillSetStateTo("validated")
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200));

        // Mock for polling response - Validation Pending
        server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/oauth2/v1/mfa/token")
                .UsingPost())
            .InScenario(validationScenario)
            .WhenStateIs("waiting_for_validation")
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(400)
                .WithBodyAsJson(new
                {
                    error = "authorization_pending",
                    error_description = "No user response received on the out-of-band authenticator yet. Continue polling to wait for a response."
                }));

        // Mock for polling response - Validation Success
        server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/oauth2/v1/mfa/token")
                .UsingPost())
            .InScenario(validationScenario)
            .WhenStateIs("validated")
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new
                {
                    expires_in = 3600,
                    id_token = "eyJhb[...]yosFQ",
                    scope = "openid",
                    token_type = "Bearer"
                }));

        return server;
    }
}