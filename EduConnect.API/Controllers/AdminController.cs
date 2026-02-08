using EduConnect.Application.Common.Models;
using EduConnect.Application.DTOs.Admin;
using EduConnect.Application.Features.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.API.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
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
            return Ok(new { userId = response.UserId, temporaryPassword = response.TemporaryPassword, message = "Teacher onboarded successfully. Share the temporary password with the teacher; they must change it on first login." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachers([FromQuery] PagedRequest? request = null)
    {
        try
        {
            if (request == null)
            {
                var teachers = await _adminService.GetTeachersAsync();
                return Ok(teachers);
            }
            else
            {
                var result = await _adminService.GetTeachersPagedAsync(request);
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
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
                return NotFound(new { error = "Teacher not found" });
            }
            return Ok(teacher);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("teachers/{id}")]
    public async Task<IActionResult> UpdateTeacher(int id, [FromBody] UpdateTeacherRequest request)
    {
        try
        {
            var result = await _adminService.UpdateTeacherAsync(id, request);
            return Ok(new { success = result, message = "Teacher updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("teachers/{id}/reset-password")]
    public async Task<IActionResult> ResetTeacherPassword(int id)
    {
        try
        {
            var response = await _adminService.ResetTeacherPasswordAsync(id);
            return Ok(new { email = response.Email, temporaryPassword = response.TemporaryPassword, message = "Password reset. Share the new temporary password with the teacher; they must change it on next login." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("teachers/{id}/verify")]
    public async Task<IActionResult> VerifyTeacher(int id)
    {
        try
        {
            var result = await _adminService.VerifyTeacherAsync(id);
            return Ok(new { success = result, message = "Teacher verified successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("teachers/{id}/reject")]
    public async Task<IActionResult> RejectTeacher(int id, [FromBody] RejectTeacherRequest request)
    {
        try
        {
            var result = await _adminService.RejectTeacherAsync(id, request.Reason);
            return Ok(new { success = result, message = "Teacher rejected successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("teachers/{id}/activate")]
    public async Task<IActionResult> SetTeacherActive(int id, [FromBody] SetActiveRequest request)
    {
        try
        {
            var result = await _adminService.SetTeacherActiveAsync(id, request.IsActive);
            return Ok(new { success = result, message = request.IsActive ? "Teacher activated" : "Teacher suspended" });
        }
        catch (Exception ex)
        {
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
            return Ok(response);
        }
        catch (Exception ex)
        {
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
                return NotFound(new { error = "Parent not found" });
            }
            return Ok(parent);
        }
        catch (Exception ex)
        {
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
            return Ok(new { studentId = id, message = "Student created successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents([FromQuery] string? parentId = null)
    {
        try
        {
            List<StudentDto> students;
            if (!string.IsNullOrEmpty(parentId))
            {
                students = await _adminService.GetStudentsByParentAsync(parentId);
            }
            else
            {
                students = await _adminService.GetStudentsAsync();
            }
            return Ok(students);
        }
        catch (Exception ex)
        {
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
                return NotFound(new { error = "Contract not found" });
            return Ok(c);
        }
        catch (Exception ex)
        {
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
            return Ok(c);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("contracts/{id}/activate")]
    public async Task<IActionResult> ActivateContract(int id)
    {
        try
        {
            await _adminService.ActivateContractAsync(id);
            return Ok(new { success = true, message = "Contract activated" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("contracts/{id}/cancel")]
    public async Task<IActionResult> CancelContract(int id)
    {
        try
        {
            await _adminService.CancelContractAsync(id);
            return Ok(new { success = true, message = "Contract cancelled" });
        }
        catch (Exception ex)
        {
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
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
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
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
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
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ——— Wallet / Payments — Master Doc B7 ———
    [HttpPost("wallet/credit")]
    public async Task<IActionResult> CreditHours([FromBody] WalletCreditRequest request)
    {
        try
        {
            var adminUserId = GetUserId() ?? throw new UnauthorizedAccessException();
            await _adminService.CreditStudentHoursAsync(request.StudentId, request.ContractId,
                new WalletAdjustRequest { Hours = request.Hours, Reason = request.Reason }, adminUserId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("wallet/deduct")]
    public async Task<IActionResult> DeductHours([FromBody] WalletDeductRequest request)
    {
        try
        {
            var adminUserId = GetUserId() ?? throw new UnauthorizedAccessException();
            await _adminService.DeductStudentHoursAsync(request.StudentId, request.ContractId,
                new WalletAdjustRequest { Hours = request.Hours, Reason = request.Reason }, adminUserId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("students/{id}/set-active")]
    public async Task<IActionResult> SetStudentActive(int id, [FromBody] SetActiveRequest request)
    {
        try
        {
            await _adminService.SetStudentActiveAsync(id, request.IsActive);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
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
