using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.API.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    [HttpPost("onboard-teacher")]
    public async Task<IActionResult> OnboardTeacher([FromBody] object request)
    {
        // TODO: Implement teacher onboarding logic
        return Ok(new { message = "Teacher onboarding endpoint - to be implemented" });
    }

    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachers()
    {
        // TODO: Implement get teachers logic
        return Ok(new { message = "Get teachers endpoint - to be implemented" });
    }

    [HttpGet("parents")]
    public async Task<IActionResult> GetParents()
    {
        // TODO: Implement get parents logic
        return Ok(new { message = "Get parents endpoint - to be implemented" });
    }
}
