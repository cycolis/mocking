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

        // Step 1: Send token request
        var tokenResponse = await PostFormDataAsync(
            $"{_baseUrl}/oauth2/v1/authentication/token",
            new TokenRequestPayload
            {
                Username = username,
                Password = password,
                ClientId = "o0123456789",
                ClientSecret = "123455asbdfdafs1234"
            }
        );

        var accessToken = ExtractProperty(tokenResponse, "access_token");

        // Step 2: Send OOB request
        var oobResponse = await PostFormDataAsync(
            $"{_baseUrl}/oauth2/v1/mfa/primary-authenticate",
            new OobRequestPayload
            {
                LoginHint = username,
                ClientId = "o0123456789",
                ClientSecret = "123455asbdfdafs1234"
            }
        );

        var oobCode = ExtractProperty(oobResponse, "oob_code");

        // Step 3: Poll for authorization
        return await PollForAuthorization(oobCode);
    }

    private async Task<JsonElement> PostFormDataAsync(string url, object payload)
    {
        var formData = payload.GetType()
                              .GetProperties()
                              .ToDictionary(
                                  prop => prop.Name,
                                  prop => prop.GetValue(payload)?.ToString() ?? string.Empty
                              );

        var formContent = new FormUrlEncodedContent(formData);

        var response = await _httpClient.PostAsync(url, formContent);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Request to {url} failed with status code {response.StatusCode}.");
            Console.WriteLine($"Error response: {content}");
            throw new Exception(content);
        }

        Console.WriteLine($"Request to {url} succeeded.");
        return JsonSerializer.Deserialize<JsonElement>(content);
    }


    private Dictionary<string, string> ConvertToFormData(object payload)
    {
        var formData = new Dictionary<string, string>();
        foreach (var property in payload.GetType().GetProperties())
        {
            var value = property.GetValue(payload)?.ToString();
            if (value != null)
                formData[property.Name] = value;
        }
        return formData;
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

            var pollRequest = new PollRequestPayload
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
                // Send polling request
                var pollResponse = await PostFormDataAsync($"{_baseUrl}/oauth2/v1/mfa/token", pollRequest);

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