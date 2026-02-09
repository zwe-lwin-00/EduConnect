using EduConnect.Shared.Extensions;
using EduConnect.Application.DTOs.Auth;
using EduConnect.Application.Features.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduConnect.API.Controllers;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService, ILogger<AuthController> logger) : base(logger)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        Logger.InformationLog("Login attempt");
        try
        {
            var response = await _authService.LoginAsync(request);
            Logger.InformationLog("Login succeeded");
            return Ok(response);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "Login failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        Logger.InformationLog("Refresh token request");
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "Refresh token failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            Logger.WarningLog("Logout called without valid user");
            return Unauthorized();
        }
        try
        {
            await _authService.LogoutAsync(userId);
            Logger.InformationLog("Logout succeeded");
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "Logout failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            Logger.WarningLog("ChangePassword called without valid user");
            return Unauthorized();
        }
        try
        {
            var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            Logger.InformationLog("Change password succeeded");
            return Ok(new { success = result, message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "Change password failed");
            return BadRequest(new { error = ex.Message });
        }
    }
}
