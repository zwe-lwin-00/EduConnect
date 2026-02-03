using EduConnect.Application.DTOs.Teacher;
using EduConnect.Application.Features.Attendance.Interfaces;
using EduConnect.Application.Features.Teachers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.API.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : BaseController
{
    private readonly ITeacherService _teacherService;
    private readonly IAttendanceService _attendanceService;

    public TeacherController(ITeacherService teacherService, IAttendanceService attendanceService)
    {
        _teacherService = teacherService;
        _attendanceService = attendanceService;
    }

    private async Task<int> GetTeacherIdAsync()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException();
        var teacherId = await _teacherService.GetTeacherIdByUserIdAsync(userId);
        if (!teacherId.HasValue)
            throw new UnauthorizedAccessException("Teacher profile not found.");
        return teacherId.Value;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _teacherService.GetDashboardAsync(teacherId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ex is UnauthorizedAccessException ? Unauthorized() : BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _teacherService.GetProfileAsync(teacherId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ex is UnauthorizedAccessException ? Unauthorized() : BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetAssignedStudents()
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _teacherService.GetAssignedStudentsAsync(teacherId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ex is UnauthorizedAccessException ? Unauthorized() : BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("sessions/today")]
    public async Task<IActionResult> GetTodaySessions()
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _teacherService.GetTodaySessionsAsync(teacherId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ex is UnauthorizedAccessException ? Unauthorized() : BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("sessions/upcoming")]
    public async Task<IActionResult> GetUpcomingSessions()
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _teacherService.GetUpcomingSessionsAsync(teacherId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ex is UnauthorizedAccessException ? Unauthorized() : BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability()
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _teacherService.GetAvailabilityAsync(teacherId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ex is UnauthorizedAccessException ? Unauthorized() : BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("availability")]
    public async Task<IActionResult> UpdateAvailability([FromBody] List<TeacherAvailabilityRequestDto> availabilities)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            await _teacherService.UpdateAvailabilityFromRequestAsync(teacherId, availabilities ?? new List<TeacherAvailabilityRequestDto>());
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return ex is UnauthorizedAccessException ? Unauthorized() : BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var sessionId = await _attendanceService.CheckInAsync(teacherId, request.ContractId);
            return Ok(new { sessionId, message = "Checked in successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var sessionId = request.SessionId;
            if (sessionId <= 0)
                return BadRequest(new { error = "SessionId is required." });
            await _attendanceService.CheckOutAsync(teacherId, sessionId, request.LessonNotes ?? string.Empty);
            return Ok(new { success = true, message = "Checked out successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
