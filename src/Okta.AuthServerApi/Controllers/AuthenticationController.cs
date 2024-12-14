using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Okta.AuthServerApi.Repositories;
using Okta.DeviceAccess.Core.Models;

namespace Okta.AuthServerApi.Controllers;

[ApiController]
[Route("oauth2/v1")]
public class AuthenticationController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IUserRepository _userRepository;

    public AuthenticationController(
        IHttpClientFactory httpClientFactory,
        IUserRepository userRepository,
        IOptions<AppSettings> options)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);

        _userRepository = userRepository;
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetToken([FromForm] TokenRequestPayload payload)
    {
        // Log the incoming payload for debugging purposes
        Console.WriteLine($"Received Token Request: ClientId={payload.ClientId}, ClientSecret={payload.ClientSecret}");

        // Check for missing ClientId and ClientSecret
        if (string.IsNullOrEmpty(payload.ClientId) || string.IsNullOrEmpty(payload.ClientSecret))
        {
            Console.WriteLine("Validation failed: Missing ClientId or ClientSecret.");

            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "One or more validation errors occurred.",
                status = 400,
                errors = new
                {
                    ClientId = "The ClientId field is required.",
                    ClientSecret = "The ClientSecret field is required."
                }
            });
        }

        if (payload.GrantType != "password")
        {
            var user = await _userRepository.GetUserAsync(payload.OobCode);

            if (user == null)
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    error_description = "User not found or invalid OOB code."
                });
            }

            if (!user.Validated)
            {
                return BadRequest(new
                {
                    error = "authorization_pending",
                    error_description = "No user response received on the out-of-band authenticator yet. Continue polling to wait for a response."
                });
            }
        }

        // Return a hardcoded response directly
        var response = new
        {
            access_token = "eyJhb[...]56Rg",
            expires_in = 3600,
            id_token = "eyJhb[...]yosFQ",
            scope = "openid",
            token_type = "Bearer"
        };

        Console.WriteLine("Token generated successfully.");
        return Ok(response);
    }

    [HttpPost("primary-authenticate")]
    public async Task<IActionResult> StartOutOfBandAuth([FromForm] OobRequestPayload payload)
    {
        // Generate a unique OOB code for the user
        var oobCode = Guid.NewGuid().ToString();

        await _userRepository.AddUserAsync(payload.LoginHint, oobCode);

        var response = new
        {
            oob_code = oobCode,
            expires_in = 300,
            interval = 5,
            channel = payload.ChannelHint,
            binding_method = "transfer",
            binding_code = "95"
        };

        _ = Task.Run(async () =>
        {
            var mockClientUrl = "/mock-client";
            var webhookPayload = new PushNotificationPayload
            {
                Username = "validation_started"
            };

            try
            {
                var result = await _httpClient.PostAsJsonAsync(mockClientUrl, webhookPayload);

                if (!result.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Validation trigger failed. StatusCode: {result.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while triggering validation: {ex.Message}");
            }
        });

        return Ok(response);
    }
}