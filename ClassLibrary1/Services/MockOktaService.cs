using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MockOktaClientLibrary.Models;

namespace MockOktaClientLibrary.Services;

public class MockOktaService : IMockOktaService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public MockOktaService(IHttpClientFactory httpClientFactory, IOptions<AppSettings> options)
    {
        _httpClient = httpClientFactory.CreateClient();
        _baseUrl = options.Value.BaseUrl;
    }

    public async Task<string> AuthenticateAsync(string username, string password)
    {
        Console.WriteLine($"Base URL: {_baseUrl}");

        // Step 1: Send token request to AuthenticationController
        var tokenRequest = new TokenRequestPayload
        {
            Username = username,
            Password = password,
            ClientId = "o0123456789",
            ClientSecret = "123455asbdfdafs1234"
        };

        var tokenFormData = new Dictionary<string, string>
        {
            { "grant_type", tokenRequest.GrantType },
            { "acr_values", tokenRequest.AcrValues },
            { "username", tokenRequest.Username },
            { "password", tokenRequest.Password },
            { "scope", tokenRequest.Scope },
            { "client_id", tokenRequest.ClientId },
            { "client_secret", tokenRequest.ClientSecret }
        };

        var tokenResponse = await _httpClient.PostAsync(
            $"{_baseUrl}/oauth2/v1/authentication/token",
            new FormUrlEncodedContent(tokenFormData));

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);

        // Extract the access_token
        string accessToken = tokenData.GetProperty("access_token").GetString();

        // Step 2: Send OOB request to MfaController
        var oobRequest = new OobRequestPayload
        {
            LoginHint = username,
            ClientId = tokenRequest.ClientId,
            ClientSecret = tokenRequest.ClientSecret
        };

        var oobFormData = new Dictionary<string, string>
        {
            { "challenge_hint", oobRequest.ChallengeHint },
            { "login_hint", oobRequest.LoginHint },
            { "channel_hint", oobRequest.ChannelHint },
            { "client_id", oobRequest.ClientId },
            { "client_secret", oobRequest.ClientSecret }
        };

        var oobResponse = await _httpClient.PostAsync(
            $"{_baseUrl}/oauth2/v1/mfa/primary-authenticate",
            new FormUrlEncodedContent(oobFormData));

        var oobContent = await oobResponse.Content.ReadAsStringAsync();
        var oobData = JsonSerializer.Deserialize<JsonElement>(oobContent);

        // Extract oob_code
        string oobCode = oobData.GetProperty("oob_code").GetString();

        // Step 3: Poll for authorization to MfaController
        while (true)
        {
            await Task.Delay(2000); // Wait for 2 seconds

            var pollRequest = new PollRequestPayload
            {
                OobCode = oobCode,
                ClientId = tokenRequest.ClientId,
                ClientSecret = tokenRequest.ClientSecret
            };

            var pollFormData = new Dictionary<string, string>
            {
                { "oob_code", pollRequest.OobCode },
                { "grant_type", pollRequest.GrantType },
                { "acr_values", pollRequest.AcrValues },
                { "scope", pollRequest.Scope },
                { "client_id", pollRequest.ClientId },
                { "client_secret", pollRequest.ClientSecret }
            };

            var pollResponse = await _httpClient.PostAsync(
                $"{_baseUrl}/oauth2/v1/mfa/token",
                new FormUrlEncodedContent(pollFormData));

            var pollContent = await pollResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Polling Response: {pollContent}");

            if (pollResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Polling succeeded.");
                return pollContent;
            }
            else
            {
                Console.WriteLine("Polling failed with status code:");
                Console.WriteLine($"Status Code: {pollResponse.StatusCode}, Reason: {pollResponse.ReasonPhrase}");
            }

            var errorData = JsonSerializer.Deserialize<JsonElement>(pollContent);
            if (errorData.TryGetProperty("error", out var errorProperty))
            {
                string error = errorProperty.GetString();
                Console.WriteLine($"Error received: {error}");
                if (error != "authorization_pending")
                {
                    throw new Exception($"Polling failed with error: {error}");
                }
            }
            else
            {
                throw new Exception("Unexpected response format. 'Error' key not found in response.");
            }
        }
    }
}