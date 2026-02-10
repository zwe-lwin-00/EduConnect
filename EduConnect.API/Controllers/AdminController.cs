using EduConnect.Shared.Extensions;
using EduConnect.Application.Common.Exceptions;
using EduConnect.Application.Common.Models;
using EduConnect.Application.DTOs.Admin;
using EduConnect.Application.DTOs.Teacher;
using EduConnect.Application.Features.Admin.Interfaces;
using EduConnect.Application.Features.GroupClass.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.API.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;
    private readonly IGroupClassService _groupClassService;

    public AdminController(IAdminService adminService, IGroupClassService groupClassService, ILogger<AdminController> logger) : base(logger)
    {
        _adminService = adminService;
        _groupClassService = groupClassService;
    }

    // ——— Dashboard — Master Doc B1 ———
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var result = await _adminService.GetDashboardAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetDashboard failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("onboard-teacher")]
    public async Task<IActionResult> OnboardTeacher([FromBody] OnboardTeacherRequest request)
    {
        try
        {
            var adminUserId = GetUserId() ?? throw new UnauthorizedAccessException();
            var response = await _adminService.OnboardTeacherAsync(request, adminUserId);
            Logger.InformationLog("Teacher onboarded successfully");
            return Ok(new { userId = response.UserId, temporaryPassword = response.TemporaryPassword, message = "Teacher onboarded successfully. Share the temporary password with the teacher; they must change it on first login." });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "OnboardTeacher failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachers(
        [FromQuery] PagedRequest? request = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? verificationStatus = null,
        [FromQuery] string? specializations = null)
    {
        try
        {
            if (request == null)
            {
                var teachers = await _adminService.GetTeachersAsync(searchTerm, verificationStatus, specializations);
                return Ok(teachers);
            }
            var result = await _adminService.GetTeachersPagedAsync(request, verificationStatus, specializations);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetTeachers failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("teachers/{id}")]
    public async Task<IActionResult> GetTeacherById(int id)
    {
        try
        {
            var teacher = await _adminService.GetTeacherByIdAsync(id);
            if (teacher == null)
            {
                Logger.WarningLog("GetTeacherById: teacher not found");
                return NotFound(new { error = "Teacher not found" });
            }
            return Ok(teacher);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetTeacherById failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("teachers/{id}")]
    public async Task<IActionResult> UpdateTeacher(int id, [FromBody] UpdateTeacherRequest request)
    {
        try
        {
            var result = await _adminService.UpdateTeacherAsync(id, request);
            Logger.InformationLog("Teacher updated successfully");
            return Ok(new { success = result, message = "Teacher updated successfully" });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "UpdateTeacher failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("teachers/{id}/reset-password")]
    public async Task<IActionResult> ResetTeacherPassword(int id)
    {
        try
        {
            var response = await _adminService.ResetTeacherPasswordAsync(id);
            Logger.InformationLog("Teacher password reset");
            return Ok(new { email = response.Email, temporaryPassword = response.TemporaryPassword, message = "Password reset. Share the new temporary password with the teacher; they must change it on next login." });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "ResetTeacherPassword failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("teachers/{id}/verify")]
    public async Task<IActionResult> VerifyTeacher(int id)
    {
        try
        {
            var result = await _adminService.VerifyTeacherAsync(id);
            Logger.InformationLog("Teacher verified");
            return Ok(new { success = result, message = "Teacher verified successfully" });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "VerifyTeacher failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("teachers/{id}/reject")]
    public async Task<IActionResult> RejectTeacher(int id, [FromBody] RejectTeacherRequest request)
    {
        try
        {
            var result = await _adminService.RejectTeacherAsync(id, request.Reason);
            Logger.InformationLog("Teacher rejected");
            return Ok(new { success = result, message = "Teacher rejected successfully" });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "RejectTeacher failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("teachers/{id}/activate")]
    public async Task<IActionResult> SetTeacherActive(int id, [FromBody] SetActiveRequest request)
    {
        try
        {
            var result = await _adminService.SetTeacherActiveAsync(id, request.IsActive);
            Logger.InformationLog(request.IsActive ? "Teacher activated" : "Teacher suspended");
            return Ok(new { success = result, message = request.IsActive ? "Teacher activated" : "Teacher suspended" });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "SetTeacherActive failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("parents")]
    public async Task<IActionResult> CreateParent([FromBody] CreateParentRequest request)
    {
        try
        {
            var adminUserId = GetUserId() ?? throw new UnauthorizedAccessException();
            var response = await _adminService.CreateParentAsync(request, adminUserId);
            Logger.InformationLog("Parent created");
            return Ok(response);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "CreateParent failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("parents")]
    public async Task<IActionResult> GetParents([FromQuery] PagedRequest? request = null)
    {
        try
        {
            if (request == null)
            {
                var parents = await _adminService.GetParentsAsync();
                return Ok(parents);
            }
            else
            {
                var result = await _adminService.GetParentsPagedAsync(request);
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetParents failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("parents/{id}")]
    public async Task<IActionResult> GetParentById(string id)
    {
        try
        {
            var parent = await _adminService.GetParentByIdAsync(id);
            if (parent == null)
            {
                Logger.WarningLog("GetParentById: parent not found");
                return NotFound(new { error = "Parent not found" });
            }
            return Ok(parent);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetParentById failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("students")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
    {
        try
        {
            var adminUserId = GetUserId() ?? throw new UnauthorizedAccessException();
            var id = await _adminService.CreateStudentAsync(request, adminUserId);
            Logger.InformationLog("Student created");
            return Ok(new { studentId = id, message = "Student created successfully" });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "CreateStudent failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents([FromQuery] string? parentId = null, [FromQuery] int? gradeLevel = null)
    {
        try
        {
            var students = await _adminService.GetStudentsAsync(parentId, gradeLevel);
            return Ok(students);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetStudents failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    // ——— Contracts — Master Doc B4 ———
    [HttpGet("contracts")]
    public async Task<IActionResult> GetContracts([FromQuery] int? teacherId, [FromQuery] int? studentId, [FromQuery] int? status)
    {
        try
        {
            var list = await _adminService.GetContractsAsync(teacherId, studentId, status);
            return Ok(list);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetContracts failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("contracts/{id}")]
    public async Task<IActionResult> GetContractById(int id)
    {
        try
        {
            var c = await _adminService.GetContractByIdAsync(id);
            if (c == null)
            {
                Logger.WarningLog("GetContractById: contract not found");
                return NotFound(new { error = "Contract not found" });
            }
            return Ok(c);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetContractById failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("contracts")]
    public async Task<IActionResult> CreateContract([FromBody] CreateContractRequest request)
    {
        try
        {
            var adminUserId = GetUserId() ?? throw new UnauthorizedAccessException();
            var c = await _adminService.CreateContractAsync(request, adminUserId);
            Logger.InformationLog("Contract created");
            return Ok(c);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "CreateContract failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("contracts/{id}/activate")]
    public async Task<IActionResult> ActivateContract(int id)
    {
        try
        {
            await _adminService.ActivateContractAsync(id);
            Logger.InformationLog("Contract activated");
            return Ok(new { success = true, message = "Contract activated" });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "ActivateContract failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("contracts/{id}/cancel")]
    public async Task<IActionResult> CancelContract(int id)
    {
        try
        {
            await _adminService.CancelContractAsync(id);
            Logger.InformationLog("Contract cancelled");
            return Ok(new { success = true, message = "Contract cancelled" });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "CancelContract failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    // ——— Group classes (admin prepares; assigns teacher) ———
    [HttpGet("group-classes")]
    public async Task<IActionResult> GetGroupClasses()
    {
        try
        {
            var list = await _groupClassService.GetAllForAdminAsync();
            return Ok(list);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetGroupClasses failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("group-classes")]
    public async Task<IActionResult> CreateGroupClass([FromBody] AdminCreateGroupClassRequest request)
    {
        try
        {
            var result = await _groupClassService.CreateAsync(request.TeacherId, request.Name ?? "", request.ZoomJoinUrl);
            Logger.InformationLog("Group class created");
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "CreateGroupClass failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("group-classes/{id}")]
    public async Task<IActionResult> GetGroupClassById(int id)
    {
        try
        {
            var result = await _groupClassService.GetByIdForAdminAsync(id);
            if (result == null)
            {
                Logger.WarningLog("GetGroupClassById: not found");
                return NotFound();
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetGroupClassById failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("group-classes/{id}")]
    public async Task<IActionResult> UpdateGroupClass(int id, [FromBody] AdminUpdateGroupClassRequest request)
    {
        try
        {
            var ok = await _groupClassService.UpdateByAdminAsync(id, request.TeacherId, request.Name ?? "", request.IsActive, request.ZoomJoinUrl);
            if (!ok)
            {
                Logger.WarningLog("UpdateGroupClass: not found");
                return NotFound();
            }
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
            var result = await _groupClassService.GetEnrollmentsForAdminAsync(id);
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
            var group = await _groupClassService.GetByIdForAdminAsync(id);
            if (group == null)
            {
                Logger.WarningLog("EnrollInGroupClass: group not found");
                return NotFound();
            }
            await _groupClassService.EnrollStudentAsync(id, group.TeacherId, request.StudentId, request.ContractId);
            Logger.InformationLog("Student enrolled in group class");
            return Ok(new { success = true });
        }
        catch (NotFoundException)
        {
            Logger.WarningLog("EnrollInGroupClass: not found (contract or student)");
            return NotFound();
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
            var ok = await _groupClassService.UnenrollByAdminAsync(enrollmentId);
            if (!ok)
            {
                Logger.WarningLog("UnenrollFromGroupClass: enrollment not found");
                return NotFound();
            }
            Logger.InformationLog("Student unenrolled from group class");
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "UnenrollFromGroupClass failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    // ——— Attendance oversight — Master Doc B6 ———
    [HttpGet("attendance/today")]
    public async Task<IActionResult> GetTodaySessions()
    {
        try
        {
            var list = await _adminService.GetTodaySessionsAsync();
            return Ok(list);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetTodaySessions failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("attendance/{id}/override-checkin")]
    public async Task<IActionResult> OverrideCheckIn(int id)
    {
        try
        {
            var adminUserId = GetUserId() ?? throw new UnauthorizedAccessException();
            await _adminService.OverrideCheckInAsync(id, adminUserId);
            Logger.InformationLog("Override check-in applied");
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "OverrideCheckIn failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("attendance/{id}/override-checkout")]
    public async Task<IActionResult> OverrideCheckOut(int id)
    {
        try
        {
            var adminUserId = GetUserId() ?? throw new UnauthorizedAccessException();
            await _adminService.OverrideCheckOutAsync(id, adminUserId);
            Logger.InformationLog("Override check-out applied");
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "OverrideCheckOut failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("attendance/{id}/adjust-hours")]
    public async Task<IActionResult> AdjustHours(int id, [FromBody] AdjustHoursRequest request)
    {
        try
        {
            var adminUserId = GetUserId() ?? throw new UnauthorizedAccessException();
            await _adminService.AdjustSessionHoursAsync(id, request, adminUserId);
            Logger.InformationLog("Session hours adjusted");
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "AdjustHours failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    // ——— Subscriptions — Master Doc B7 (monthly only) ———
    [HttpPost("contracts/{id}/renew-subscription")]
    public async Task<IActionResult> RenewSubscription(int id)
    {
        try
        {
            var adminUserId = GetUserId() ?? throw new UnauthorizedAccessException();
            await _adminService.RenewSubscriptionAsync(id, adminUserId);
            Logger.InformationLog("Subscription renewed");
            return Ok(new { success = true, message = "Subscription renewed for one month." });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "RenewSubscription failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("students/{id}/set-active")]
    public async Task<IActionResult> SetStudentActive(int id, [FromBody] SetActiveRequest request)
    {
        try
        {
            await _adminService.SetStudentActiveAsync(id, request.IsActive);
            Logger.InformationLog("Student active status updated");
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "SetStudentActive failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    // ——— Reports — Master Doc B8 ———
    [HttpGet("reports/daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime? date)
    {
        try
        {
            var d = date ?? EduConnect.Infrastructure.MyanmarTimeHelper.GetTodayInMyanmar();
            var result = await _adminService.GetDailyReportAsync(d);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetDailyReport failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("reports/monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var result = await _adminService.GetMonthlyReportAsync(year, month);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog(ex, "GetMonthlyReport failed");
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class SetActiveRequest
{
    public bool IsActive { get; set; }
}

public class RejectTeacherRequest
{
    public string Reason { get; set; } = string.Empty;
}
