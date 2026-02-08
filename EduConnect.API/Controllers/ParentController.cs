using EduConnect.Application.Features.Parents.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.API.Controllers;

[Authorize(Roles = "Parent")]
public class ParentController : BaseController
{
    private readonly IParentService _parentService;

    public ParentController(IParentService parentService)
    {
        _parentService = parentService;
    }

    [HttpGet("my-students")]
    public async Task<IActionResult> GetMyStudents()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _parentService.GetMyStudentsAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
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
                return Unauthorized();
            var result = await _parentService.GetStudentLearningOverviewAsync(userId, studentId);
            if (result == null)
                return NotFound(new { error = "Student not found or access denied." });
            return Ok(result);
        }
        catch (Exception ex)
        {
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
                return Unauthorized();
            var monday = ParseWeekStartMonday(weekStart);
            var result = await _parentService.GetSessionsForStudentWeekAsync(userId, studentId, monday);
            return Ok(result);
        }
        catch (Exception ex)
        {
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
