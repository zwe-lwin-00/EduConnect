using Microsoft.AspNetCore.Mvc;

namespace EduConnect.API.Controllers;

public class AuthController : BaseController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] object request)
    {
        // TODO: Implement login logic
        return Ok(new { message = "Login endpoint - to be implemented" });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] object request)
    {
        // TODO: Implement refresh token logic
        return Ok(new { message = "Refresh token endpoint - to be implemented" });
    }
}
