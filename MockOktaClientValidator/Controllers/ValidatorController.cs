namespace MockOktaClientValidator.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("mock-client")]
    public class ValidatorController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ValidatorController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("trigger")]
        public async Task<IActionResult> TriggerValidation([FromBody] string username)
        {
            Console.WriteLine($"Triggering validation for user: {username}");

            await Task.Delay(5000);

            var client = _httpClientFactory.CreateClient();
            var apiUrl = "http://localhost:5114/oauth2/v1/validate";

            var response = await client.PostAsync(apiUrl, new StringContent($"\"{username}\"", System.Text.Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Validation triggered successfully.");
                return Ok("Validation triggered successfully.");
            }

            Console.WriteLine($"Validation trigger failed. StatusCode: {response.StatusCode}");
            return StatusCode((int)response.StatusCode, "Validation trigger failed.");
        }
    }
}
