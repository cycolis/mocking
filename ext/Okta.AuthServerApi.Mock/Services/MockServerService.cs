﻿using System.Net.Http.Json;
using Microsoft.Extensions.Options;

using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using WireMock.Types;
using WireMock.Util;

using Okta.DeviceAccess.Core.Models;

namespace Okta.AuthServerApi.Mock.Services;

internal class MockServerService : IMockServerService
{
    private const string VALIDATION_SCENARIO = "ValidationFlow";
    private readonly HttpClient _httpClient;
    private readonly WireMockServer _server;

    public MockServerService(
        IHttpClientFactory httpClientFactory,
        IOptions<WireMockAppSettings> options)
    {
        var settings = options.Value;

        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(settings.BaseUrl);

        _server = WireMockServer.Start(new WireMockServerSettings
        {
            Urls = [$"http://0.0.0.0:{settings.WireMockPort}"],
            StartAdminInterface = true,
            ReadStaticMappings = true
        });

        SetupAuthBasedOnUserPw();
        SetupOobEndpoint();
        SetupValidateEndpoint();

        SetupPollingEndpointWhenWaitingForValidation();
        SetupPollingEndpointWhenValidated();
        SetupPollingEndpointWhenInvalidated();
    }

    public string Url => _server.Urls[0];

    public void Stop() => _server.Stop();

    private void SetupAuthBasedOnUserPw()
    {
        _server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/token")
                .UsingPost()
                .WithBody(b => b.Contains("\"grant_type\":\"password\"") || b.Contains("\"grant_type\": \"password\"")))
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

    private void SetupOobEndpoint()
    { 
        _server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/primary-authenticate")
                .UsingPost())
            .InScenario(VALIDATION_SCENARIO)
            .WhenStateIs("Started")
            .WillSetStateTo("waiting_for_validation")
            .RespondWith(Response.Create()
                .WithCallback(request => GetResponse()));
    }

    private ResponseMessage GetResponse()
    {
        // Trigger an outgoing HTTP request (Webhook simulation)
        Console.WriteLine("Incoming request received. Triggering webhook...");

        SendNotificationToMobile();

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

    private void SendNotificationToMobile()
    {
        var webhookPayload = new PushNotificationPayload
        {
            Username = "validation_started"
        };

        var task = Task.Run(async () =>
        {
            await _httpClient.PostAsJsonAsync(
                "//mock-client", webhookPayload);
        });
        task.Wait();
    }

    private void SetupValidateEndpoint()
    {
        _server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/mfa/validate")
                .UsingPost())
            .InScenario(VALIDATION_SCENARIO)
            .WhenStateIs("waiting_for_validation")
            .WillSetStateTo("validated")
            .RespondWith(Response.Create()
                .WithStatusCode(200));
    }

    private void SetupPollingEndpointWhenWaitingForValidation()
    {
        _server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/token")
                .UsingPost()
                .WithBody(b => b.Contains("\"grant_type\":\"urn:okta:params:oauth:grant-type:oob\"")))
            .InScenario(VALIDATION_SCENARIO)
            .WhenStateIs("waiting_for_validation")
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithBodyAsJson(new
                {
                    error = "authorization_pending",
                    error_description = "No user response received on the out-of-band authenticator yet. Continue polling to wait for a response."
                }));
    }

    private void SetupPollingEndpointWhenValidated()
    {
        _server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/token")
                .UsingPost()
                .WithBody(b => b.Contains("\"grant_type\":\"urn:okta:params:oauth:grant-type:oob\"")))
            .InScenario(VALIDATION_SCENARIO)
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

    private void SetupPollingEndpointWhenInvalidated()
    {
        _server
            .Given(Request.Create()
                .WithPath("/oauth2/v1/token")
                .UsingPost()
                .WithBody(b => b.Contains("\"grant_type\":\"urn:okta:params:oauth:grant-type:oob\"")))
            .InScenario(VALIDATION_SCENARIO)
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