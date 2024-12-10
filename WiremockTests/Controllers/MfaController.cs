using Microsoft.AspNetCore.Mvc;
using MockOktaApi.Repositories;
using MockOktaClientLibrary.Models;
using System.Net.Http;

namespace MockOktaApi.Controllers;

[ApiController]
[Route("oauth2/v1/mfa")]
public class MfaController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IUserRepository _userRepository;

    public MfaController(IHttpClientFactory httpClientFactory, IUserRepository userRepository)
    {
        _httpClient = httpClientFactory.CreateClient();
        _userRepository = userRepository;
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
            var mockClientUrl = "http://localhost:5073/mock-client/trigger";
            var content = new StringContent($"\"{payload.LoginHint}\"", System.Text.Encoding.UTF8, "application/json");

            try
            {
                await Task.Delay(5000); // Simulate a delay of 5 seconds
                var result = await _httpClient.PostAsync(mockClientUrl, content);

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


    [HttpPost("token")]
    public async Task<IActionResult> PollAuthorization([FromForm] PollRequestPayload payload)
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

        return Ok(new
        {
            expires_in = 3600,
            access_token = "eyJhb[...]56Rg",
            id_token = "eyJhb[...]yosFQ",
            scope = "openid",
            token_type = "Bearer"
        });
    }

}