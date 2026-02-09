using Microsoft.AspNetCore.Mvc;

namespace EduConnect.API.Controllers;

/// <summary>
/// Legacy health endpoint (status only). Prefer /health/live (liveness) and /health/ready (readiness with DB) for containers and orchestration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "EduConnect API",
            endpoints = new { liveness = "/health/live", readiness = "/health/ready" }
        });
    }
}
