using Microsoft.AspNetCore.Mvc;
using MockOktaClientLibrary.Models;

namespace MockOktaApi.Controllers;

[ApiController]
[Route("oauth2/v1/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public AuthenticationController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpPost("token")]
    public IActionResult GetToken([FromForm] TokenRequestPayload payload)
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
}