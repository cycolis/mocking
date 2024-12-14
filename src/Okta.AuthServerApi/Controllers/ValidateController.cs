using Microsoft.AspNetCore.Mvc;
using Okta.AuthServerApi.Repositories;

namespace MockOktaApi.Controllers;

[ApiController]
[Route("oauth2/v1")]
public class ValidateController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public ValidateController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateUser([FromBody] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Username is required.");
        }

        var isValidated = await _userRepository.SetUserValidatedAsync(username);
        if (!isValidated)
        {
            return NotFound("User not found or already validated.");
        }

        return Ok("User validated.");
    }
}
