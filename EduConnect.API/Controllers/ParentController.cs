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
}
