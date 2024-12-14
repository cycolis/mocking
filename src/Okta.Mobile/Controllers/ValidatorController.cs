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
    }

    [HttpPost]
    public async Task<IActionResult> TriggerValidation([FromBody] string username)
    {
        Console.WriteLine($"Triggering validation for user: {username}");

        await Task.Delay(5000);

        var apiUrl = "/oauth2/v1/validate";

        var response = await _httpClient.PostAsync(apiUrl, new StringContent($"\"{username}\"", System.Text.Encoding.UTF8, "application/json"));
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Validation triggered successfully.");
            return Ok("Validation triggered successfully.");
        }

        Console.WriteLine($"Validation trigger failed. StatusCode: {response.StatusCode}");
        return StatusCode((int)response.StatusCode, "Validation trigger failed.");
    }
}

