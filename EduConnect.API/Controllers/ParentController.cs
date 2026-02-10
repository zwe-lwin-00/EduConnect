using EduConnect.Shared.Extensions;
using EduConnect.Application.Features.Admin.Interfaces;
using EduConnect.Application.Features.Parents.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.API.Controllers;

[Authorize(Roles = "Parent")]
public class ParentController : BaseController
{
    private readonly IParentService _parentService;
    private readonly ISettingsService _settingsService;

    public ParentController(IParentService parentService, ISettingsService settingsService, ILogger<ParentController> logger) : base(logger)
    {
        _parentService = parentService;
        _settingsService = settingsService;
    }

    [HttpGet("my-students")]
    public async Task<IActionResult> GetMyStudents()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Logger.WarningLog("GetMyStudents: unauthorized");
                return Unauthorized();
            }
            var result = await _parentService.GetMyStudentsAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetMyStudents failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("my-students/{studentId}/learning-overview")]
    public async Task<IActionResult> GetStudentLearningOverview(int studentId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Logger.WarningLog("GetStudentLearningOverview: unauthorized");
                return Unauthorized();
            }
            var result = await _parentService.GetStudentLearningOverviewAsync(userId, studentId);
            if (result == null)
            {
                Logger.WarningLog("GetStudentLearningOverview: student not found or access denied");
                return NotFound(new { error = "Student not found or access denied." });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetStudentLearningOverview failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("my-students/{studentId}/calendar/week")]
    public async Task<IActionResult> GetStudentCalendarWeek(int studentId, [FromQuery] string? weekStart)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Logger.WarningLog("GetStudentCalendarWeek: unauthorized");
                return Unauthorized();
            }
            var monday = ParseWeekStartMonday(weekStart);
            var result = await _parentService.GetSessionsForStudentWeekAsync(userId, studentId, monday);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetStudentCalendarWeek failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("my-students/{studentId}/calendar/month")]
    public async Task<IActionResult> GetStudentCalendarMonth(int studentId, [FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Logger.WarningLog("GetStudentCalendarMonth: unauthorized");
                return Unauthorized();
            }
            if (year < 2000 || year > 2100 || month < 1 || month > 12)
                return BadRequest(new { error = "Invalid year or month." });
            var result = await _parentService.GetSessionsForStudentMonthAsync(userId, studentId, year, month);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetStudentCalendarMonth failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("holidays")]
    public async Task<IActionResult> GetHolidays([FromQuery] int? year)
    {
        try
        {
            var list = await _settingsService.GetHolidaysAsync(year);
            return Ok(list);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetHolidays failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    private static DateTime ParseWeekStartMonday(string? weekStart)
    {
        if (!string.IsNullOrEmpty(weekStart) && DateTime.TryParse(weekStart, out var parsed))
            return parsed.Date;
        var today = EduConnect.Infrastructure.MyanmarTimeHelper.GetTodayInMyanmar();
        return EduConnect.Infrastructure.MyanmarTimeHelper.GetWeekStartMonday(today);
    }
}
