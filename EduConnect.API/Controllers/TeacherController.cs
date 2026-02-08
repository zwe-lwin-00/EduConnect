using EduConnect.Application.DTOs.Teacher;
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

    public TeacherController(ITeacherService teacherService, IAttendanceService attendanceService, IHomeworkService homeworkService, IGroupClassService groupClassService)
    {
        _teacherService = teacherService;
        _attendanceService = attendanceService;
        _homeworkService = homeworkService;
        _groupClassService = groupClassService;
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
            return ex is UnauthorizedAccessException ? Unauthorized() : BadRequest(new { error = ex.Message });
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

    [HttpPost("check-in/group")]
    public async Task<IActionResult> CheckInGroup([FromBody] GroupCheckInRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var sessionId = await _attendanceService.CheckInGroupAsync(teacherId, request.GroupClassId);
            return Ok(new { sessionId, message = "Group session started." });
        }
        catch (Exception ex)
        {
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
            return Ok(new { success = true, message = "Group session completed." });
        }
        catch (Exception ex)
        {
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
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("group-classes")]
    public async Task<IActionResult> CreateGroupClass([FromBody] CreateGroupClassRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var result = await _groupClassService.CreateAsync(teacherId, request.Name ?? "");
            return Ok(result);
        }
        catch (Exception ex)
        {
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
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("group-classes/{id}")]
    public async Task<IActionResult> UpdateGroupClass(int id, [FromBody] UpdateGroupClassRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            var ok = await _groupClassService.UpdateAsync(id, teacherId, request.Name ?? "", request.IsActive);
            if (!ok) return NotFound();
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
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
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("group-classes/{id}/enroll")]
    public async Task<IActionResult> EnrollInGroupClass(int id, [FromBody] EnrollInGroupClassRequest request)
    {
        try
        {
            var teacherId = await GetTeacherIdAsync();
            await _groupClassService.EnrollStudentAsync(id, teacherId, request.StudentId, request.ContractId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
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
            if (!ok) return NotFound();
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
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
            return Unauthorized();
        }
        catch (Exception ex)
        {
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
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
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
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
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
            return Unauthorized();
        }
        catch (Exception ex)
        {
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
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
