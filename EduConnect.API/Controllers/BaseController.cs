using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected ILogger Logger { get; }

    protected BaseController(ILogger logger)
    {
        Logger = logger;
    }

    protected string? GetUserId() => User?.FindFirstValue(ClaimTypes.NameIdentifier);
    protected bool IsAdmin() => User?.IsInRole("Admin") ?? false;
    protected bool IsTeacher() => User?.IsInRole("Teacher") ?? false;
    protected bool IsParent() => User?.IsInRole("Parent") ?? false;
}
