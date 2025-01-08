using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.Extensions.Options;

using Okta.DeviceAccess.Core.Interfaces;
using Okta.DeviceAccess.Core.Models;

namespace Okta.DeviceAccess.Core.Services;

public class OktaService : IOktaService
{
    private readonly HttpClient _httpClient;

    public OktaService(IHttpClientFactory httpClientFactory, IOptions<AppSettings> options)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    public async Task<string> AuthenticateAsync(string username, string password)
    {
        try
        {
            // Step 1: Send token request
            var tokenResponse = await PostToken(
                "/oauth2/v1/token",
                new TokenRequestPayload
                {
                    GrantType = "password",
                    AcrValues = "urn:okta:app:mfa:attestation",
                    Username = username,
                    Password = password,
                    Scope = "openid",
                    ClientId = "o0123456789",
                    ClientSecret = "123455asbdfdafs1234"
                }
            );

            var accessToken = ExtractProperty(tokenResponse, "access_token");

            // Step 2: Send OOB request
            OobRequestPayload oobRequest = new()
            {
                ChallengeHint = "urn:okta:params:oauth:grant-type:oob",
                ChannelHint = "push",
                LoginHint = username,
                ClientId = "o0123456789",
                ClientSecret = "123455asbdfdafs1234"
            };
            var response = await _httpClient.PostAsJsonAsync("/oauth2/v1/primary-authenticate", oobRequest);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("primary-authenticate");
            }

            var content = await response.Content.ReadAsStringAsync();
            var oobResponse = JsonSerializer.Deserialize<JsonElement>(content);

            var oobCode = ExtractProperty(oobResponse, "oob_code");

            // Step 3: Poll for authorization
            return await PollForAuthorization(oobCode);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Authentication failed: {ex.Message}");
            return "401 Unauthorized: Invalid credentials.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during authentication: {ex.Message}");
            return ex.Message;
        }
    }

    private async Task<JsonElement> PostToken(string url, TokenRequestPayload payload)
    {
        var response = await _httpClient.PostAsJsonAsync(url, payload);
        var content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Console.WriteLine($"Unauthorized: {url}. Payload: {JsonSerializer.Serialize(payload)}");
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Request to {url} failed with status code {response.StatusCode}.");
            Console.WriteLine($"Error response: {content}");
            throw new Exception(content);
        }

        Console.WriteLine($"Request to {url} succeeded.");
        return JsonSerializer.Deserialize<JsonElement>(content);
    }

    private string ExtractProperty(JsonElement response, string propertyName)
    {
        if (!response.TryGetProperty(propertyName, out var property))
        {
            throw new Exception($"Response does not contain '{propertyName}'.");
        }
        return property.GetString();
    }

    public async Task<string> PollForAuthorization(string oobCode)
    {
        int maxAttempts = 10;
        int attempt = 0;

        while (attempt < maxAttempts)
        {
            attempt++;
            Console.WriteLine($"Polling Attempt: {attempt}");

            var pollRequest = new TokenRequestPayload
            {
                OobCode = oobCode,
                GrantType = "urn:okta:params:oauth:grant-type:oob",
                AcrValues = "urn:okta:app:mfa:attestation",
                Scope = "openid",
                ClientId = "o0123456789",
                ClientSecret = "123455asbdfdafs1234"
            };

            try
            {
                Task.Delay(1000).Wait();

                // Send polling request
                var pollResponse = await PostToken("/oauth2/v1/token", pollRequest);

                // If polling succeeds, return the response
                Console.WriteLine($"Polling Succeeded: {pollResponse}");
                return pollResponse.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Polling failed: {ex.Message}");

                if (ex.Message.Contains("authorization_pending"))
                {
                    // Wait before retrying
                    await Task.Delay(2000);
                    continue; // Continue polling
                }

                throw;
            }
        }

        throw new Exception("Polling failed: Maximum attempts reached.");
    }
}