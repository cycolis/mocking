using System.Net.Http.Json;

using WireMock;
using WireMock.Server;
using WireMock.Settings;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Types;
using WireMock.Util;

using Okta.DeviceAccess.Core.Models;

namespace Okta.AuthServerApi.Mock;

public static class MockServerConfig
{
    public static WireMockServer CreateMockServer(HttpClient httpClient, string port)
    {
        var server = WireMockServer.Start(new WireMockServerSettings
        {
            Urls = new[] { $"http://localhost:{port}" },
            StartAdminInterface = true,
            ReadStaticMappings = true
        });

        const string validationScenario = "ValidationFlow";

        SetupAuthBasedOnUserPw(server);
        SetupOobEndpoint(server, validationScenario, httpClient);
        SetupValidateEndpoint(server, validationScenario);

        SetupPollingEndpointWhenWaitingForValidation(server, validationScenario);
        SetupPollingEndpointWhenValidated(server, validationScenario);
        SetupPollingEndpointWhenInvalidated(server, validationScenario);

        return server;
    }

    private static void SetupAuthBasedOnUserPw(WireMockServer server)
    {
        server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/token")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new
                {
                    access_token = "1eyJhb[...]56Rg",
                    expires_in = 3600,
                    id_token = "eyJhb[...]yosFQ",
                    scope = "openid",
                    token_type = "Bearer"
                }));
    }

    private static void SetupOobEndpoint(WireMockServer server, string validationScenario, HttpClient httpClient)
    {
        server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/primary-authenticate")
                .UsingPost())
            .InScenario(validationScenario)
            .WhenStateIs("Started")
            .WillSetStateTo("waiting_for_validation")
            .RespondWith(Response.Create()
                .WithCallback(request => GetResponse(httpClient)));
    }

    private static ResponseMessage GetResponse(HttpClient httpClient)
    {
        // Trigger an outgoing HTTP request (Webhook simulation)
        Console.WriteLine("Incoming request received. Triggering webhook...");

        SendNotificationToMobile(httpClient);

        // Return the desired response
        // Return the desired response
        var responseBody = new
        {
            oob_code = "ftpvP1LB26vCARL7EWM66cUhPA2vdQmHFp",
            expires_in = 300,
            interval = 5,
            channel = "push",
            binding_method = "transfer",
            binding_code = "95"
        };

        return new ResponseMessage
        {
            StatusCode = 200,
            BodyData = new BodyData
            {
                DetectedBodyType = BodyType.Json,
                BodyAsJson = responseBody
            },
            Headers = new Dictionary<string, WireMockList<string>>
                        {
                            { "Content-Type", "application/json" }
                        }
        };
    }

    private static void SendNotificationToMobile(HttpClient httpClient)
    {
        var webhookPayload = new PushNotificationPayload
        {
            Username = "validation_started"
        };

        var task = Task.Run(async () =>
        {
            await httpClient.PostAsJsonAsync(
                "//mock-client", webhookPayload);
        });
        task.Wait();
    }

    private static void SetupValidateEndpoint(WireMockServer server, string validationScenario)
    {
        server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/mfa/validate")
                .UsingPost())
            .InScenario(validationScenario)
            .WhenStateIs("waiting_for_validation")
            .WillSetStateTo("validated")
            .RespondWith(Response.Create()
                .WithStatusCode(200));
    }

    private static void SetupPollingEndpointWhenWaitingForValidation(WireMockServer server, string validationScenario)
    {
        server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/mfa/token")
                .UsingPost())
            .InScenario(validationScenario)
            .WhenStateIs("waiting_for_validation")
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithBodyAsJson(new
                {
                    error = "authorization_pending",
                    error_description = "No user response received on the out-of-band authenticator yet. Continue polling to wait for a response."
                }));
    }

    private static void SetupPollingEndpointWhenValidated(WireMockServer server, string validationScenario)
    {
        server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/mfa/token")
                .UsingPost())
            .InScenario(validationScenario)
            .WhenStateIs("validated")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new
                {
                    expires_in = 3600,
                    id_token = "eyJhb[...]yosFQ",
                    scope = "openid",
                    token_type = "Bearer"
                }));
    }

    private static void SetupPollingEndpointWhenInvalidated(WireMockServer server, string validationScenario)
    {
        server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/mfa/token")
                .UsingPost())
            .InScenario(validationScenario)
            .WhenStateIs("invalidated")
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithBodyAsJson(new
                {
                    error = "authorization_invalid",
                    error_description = "The mobile app submitted a wrong code"
                }));
    }
}