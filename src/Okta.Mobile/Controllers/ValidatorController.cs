using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Okta.DeviceAccess.Core.Models;

namespace Okta.Mobile.Controllers;

[ApiController]
[Route("mock-client")]
public class ValidatorController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public ValidatorController (
        IHttpClientFactory httpClientFactory,
        IOptions<AppSettings> options)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);

        Console.WriteLine(_httpClient.BaseAddress.ToString());
    }

    [HttpPost]
    public async Task<IActionResult> TriggerValidation([FromBody] PushNotificationPayload payload)
    {
        Console.WriteLine($"Triggering validation for user: {payload.Username}");

        // Immediately return a response to the client
        var message = "Validation will be triggered shortly.";
        Console.WriteLine("Returning response to client.");
        Response.StatusCode = StatusCodes.Status202Accepted;

        // Trigger the delayed POST call in the background
        _ = Task.Run(async () =>
        {
            await Task.Delay(5000); // Wait for 5 seconds

            var apiUrl = "/oauth2/v1/validate";
            try
            {
                var response = await _httpClient.PostAsync(
                    apiUrl,
                    new StringContent($"\"{payload.Username}\"", System.Text.Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Validation triggered successfully.");
                }
                else
                {
                    Console.WriteLine($"Validation trigger failed. StatusCode: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while triggering validation: {ex.Message}");
            }
        });

        return Ok(message);
    }
}

