using EduConnect.Shared.Extensions;
using EduConnect.Application.Common.Exceptions;
using EduConnect.Application.DTOs.Teacher;
using EduConnect.Application.Features.Admin.Interfaces;
using EduConnect.Application.Features.Attendance.Interfaces;
using EduConnect.Application.Features.GroupClass.Interfaces;
using EduConnect.Application.Features.Homework.Interfaces;
using EduConnect.Application.Features.Teachers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.API.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : BaseController
{
    private readonly ITeacherService _teacherService;
    private readonly IAttendanceService _attendanceService;
    private readonly IHomeworkService _homeworkService;
    private readonly IGroupClassService _groupClassService;
    private readonly ISettingsService _settingsService;

    public TeacherController(ITeacherService teacherService, IAttendanceService attendanceService, IHomeworkService homeworkService, IGroupClassService groupClassService, ISettingsService settingsService, ILogger<TeacherController> logger) : base(logger)
    {
        _teacherService = teacherService;
        _attendanceService = attendanceService;
        _homeworkService = homeworkService;
        _groupClassService = groupClassService;
        _settingsService = settingsService;
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
            if (ex is UnauthorizedAccessException) { Logger.WarningLog("GetDashboard: unauthorized"); return Unauthorized(); }
            Logger.ErrorLog(ex, "GetDashboard failed");
            return BadRequest(new { error = ex.Message });
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
            if (ex is UnauthorizedAccessException) { Logger.WarningLog("GetProfile: unauthorized"); return Unauthorized(); }
            Logger.ErrorLog(ex, "GetProfile failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("profile/zoom")]
    public async Task<IActionResult> UpdateZoomJoinUrl([FromBody] UpdateZoomJoinUrlRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            await _teacherService.UpdateZoomJoinUrlAsync(teacherId, request.ZoomJoinUrl);
            Logger.InformationLog("Zoom URL updated");
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            if (ex is NotFoundException) { Logger.WarningLog("UpdateZoomJoinUrl: not found"); return NotFound(); }
            Logger.ErrorLog(ex, "UpdateZoomJoinUrl failed");
            return BadRequest(new { error = ex.Message });
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
            if (ex is UnauthorizedAccessException) { Logger.WarningLog("GetAssignedStudents: unauthorized"); return Unauthorized(); }
            Logger.ErrorLog(ex, "GetAssignedStudents failed");
            return BadRequest(new { error = ex.Message });
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
            if (ex is UnauthorizedAccessException) { Logger.WarningLog("GetTodaySessions: unauthorized"); return Unauthorized(); }
            Logger.ErrorLog(ex, "GetTodaySessions failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("calendar/week")]
    public async Task<IActionResult> GetCalendarWeek([FromQuery] string? weekStart)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var monday = ParseWeekStartMonday(weekStart);
            var result = await _teacherService.GetSessionsForWeekAsync(teacherId, monday);
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex is UnauthorizedAccessException) { Logger.WarningLog("GetCalendarWeek: unauthorized"); return Unauthorized(); }
            Logger.ErrorLog(ex, "GetCalendarWeek failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("calendar/month")]
    public async Task<IActionResult> GetCalendarMonth([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            if (year < 2000 || year > 2100 || month < 1 || month > 12)
                return BadRequest(new { error = "Invalid year or month." });
            var result = await _teacherService.GetSessionsForMonthAsync(teacherId, year, month);
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex is UnauthorizedAccessException) { Logger.WarningLog("GetCalendarMonth: unauthorized"); return Unauthorized(); }
            Logger.ErrorLog(ex, "GetCalendarMonth failed");
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
            if (ex is UnauthorizedAccessException) { Logger.WarningLog("GetUpcomingSessions: unauthorized"); return Unauthorized(); }
            Logger.ErrorLog(ex, "GetUpcomingSessions failed");
            return BadRequest(new { error = ex.Message });
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
            if (ex is UnauthorizedAccessException) { Logger.WarningLog("GetAvailability: unauthorized"); return Unauthorized(); }
            Logger.ErrorLog(ex, "GetAvailability failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("availability")]
    public async Task<IActionResult> UpdateAvailability([FromBody] List<TeacherAvailabilityRequestDto> availabilities)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            await _teacherService.UpdateAvailabilityFromRequestAsync(teacherId, availabilities ?? new List<TeacherAvailabilityRequestDto>());
            Logger.InformationLog("Availability updated");
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            if (ex is UnauthorizedAccessException) { Logger.WarningLog("UpdateAvailability: unauthorized"); return Unauthorized(); }
            Logger.ErrorLog(ex, "UpdateAvailability failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var sessionId = await _attendanceService.CheckInAsync(teacherId, request.ContractId);
            Logger.InformationLog("Check-in succeeded");
            return Ok(new { sessionId, message = "Checked in successfully." });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "CheckIn failed");
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
            {
                Logger.WarningLog("CheckOut: SessionId required");
                return BadRequest(new { error = "SessionId is required." });
            }
            await _attendanceService.CheckOutAsync(teacherId, sessionId, request.LessonNotes ?? string.Empty);
            Logger.InformationLog("Check-out succeeded");
            return Ok(new { success = true, message = "Checked out successfully." });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "CheckOut failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("check-in/group")]
    public async Task<IActionResult> CheckInGroup([FromBody] GroupCheckInRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var sessionId = await _attendanceService.CheckInGroupAsync(teacherId, request.GroupClassId);
            Logger.InformationLog("Group check-in succeeded");
            return Ok(new { sessionId, message = "Group session started." });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "CheckInGroup failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("check-out/group")]
    public async Task<IActionResult> CheckOutGroup([FromBody] GroupCheckOutRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            await _attendanceService.CheckOutGroupAsync(teacherId, request.GroupSessionId, request.LessonNotes ?? string.Empty);
            Logger.InformationLog("Group check-out succeeded");
            return Ok(new { success = true, message = "Group session completed." });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "CheckOutGroup failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("group-sessions")]
    public async Task<IActionResult> GetGroupSessions([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _attendanceService.GetGroupSessionsByTeacherAsync(teacherId, from, to);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetGroupSessions failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("group-classes")]
    public async Task<IActionResult> GetGroupClasses()
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _groupClassService.GetByTeacherAsync(teacherId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetGroupClasses failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("group-classes/{id}")]
    public async Task<IActionResult> GetGroupClassById(int id)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _groupClassService.GetByIdAsync(id, teacherId);
            if (result == null) { Logger.WarningLog("GetGroupClassById: not found"); return NotFound(); }
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetGroupClassById failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("group-classes/{id}")]
    public async Task<IActionResult> UpdateGroupClass(int id, [FromBody] UpdateGroupClassRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var ok = await _groupClassService.UpdateAsync(id, teacherId, request.Name ?? "", request.IsActive, request.ZoomJoinUrl);
            if (!ok) { Logger.WarningLog("UpdateGroupClass: not found"); return NotFound(); }
            Logger.InformationLog("Group class updated");
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "UpdateGroupClass failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("group-classes/{id}/enrollments")]
    public async Task<IActionResult> GetGroupClassEnrollments(int id)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _groupClassService.GetEnrollmentsAsync(id, teacherId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetGroupClassEnrollments failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("group-classes/{id}/enroll")]
    public async Task<IActionResult> EnrollInGroupClass(int id, [FromBody] EnrollInGroupClassRequest request)
    {
        try
        {
            if (!request.ContractId.HasValue)
                return BadRequest(new { error = "ContractId is required for teacher enrollment." });
            var teacherId = await GetTeacherIdAsync();
            await _groupClassService.EnrollStudentAsync(id, teacherId, request.StudentId, request.ContractId.Value);
            Logger.InformationLog("Student enrolled in group class");
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "EnrollInGroupClass failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("group-classes/enrollments/{enrollmentId}")]
    public async Task<IActionResult> UnenrollFromGroupClass(int enrollmentId)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var ok = await _groupClassService.UnenrollAsync(enrollmentId, teacherId);
            if (!ok) { Logger.WarningLog("UnenrollFromGroupClass: not found"); return NotFound(); }
            Logger.InformationLog("Student unenrolled from group class");
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "UnenrollFromGroupClass failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("homework")]
    public async Task<IActionResult> GetHomeworks([FromQuery] int? studentId = null, [FromQuery] DateTime? dueDateFrom = null, [FromQuery] DateTime? dueDateTo = null)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _homeworkService.GetHomeworksByTeacherAsync(teacherId, studentId, dueDateFrom, dueDateTo);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            Logger.WarningLog("GetHomeworks: unauthorized");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetHomeworks failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("homework")]
    public async Task<IActionResult> CreateHomework([FromBody] CreateHomeworkRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _homeworkService.CreateHomeworkAsync(teacherId, request);
            Logger.InformationLog("Homework created");
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            Logger.WarningLog("CreateHomework: unauthorized");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "CreateHomework failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("homework/{homeworkId}/status")]
    public async Task<IActionResult> UpdateHomeworkStatus(int homeworkId, [FromBody] UpdateHomeworkStatusRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _homeworkService.UpdateHomeworkStatusAsync(teacherId, homeworkId, request);
            if (result == null) { Logger.WarningLog("UpdateHomeworkStatus: not found"); return NotFound(); }
            Logger.InformationLog("Homework status updated");
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            Logger.WarningLog("UpdateHomeworkStatus: unauthorized");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "UpdateHomeworkStatus failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("grades")]
    public async Task<IActionResult> GetGrades([FromQuery] int? studentId = null, [FromQuery] DateTime? gradeDateFrom = null, [FromQuery] DateTime? gradeDateTo = null)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _homeworkService.GetGradesByTeacherAsync(teacherId, studentId, gradeDateFrom, gradeDateTo);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            Logger.WarningLog("GetGrades: unauthorized");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetGrades failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("grades")]
    public async Task<IActionResult> CreateGrade([FromBody] CreateGradeRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _homeworkService.CreateGradeAsync(teacherId, request);
            Logger.InformationLog("Grade created");
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            Logger.WarningLog("CreateGrade: unauthorized");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "CreateGrade failed");
            return BadRequest(new { error = ex.Message });
        }
    }
}
