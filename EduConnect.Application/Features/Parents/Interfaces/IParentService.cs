using EduConnect.Application.Common.Interfaces;
using EduConnect.Application.DTOs.Parent;

namespace EduConnect.Application.Features.Parents.Interfaces;

/// <summary>
/// Parent-facing service — Master Doc 11C. Students have no login; all data via Parent. Read-only + purchase.
/// </summary>
public interface IParentService : IService
{
    Task<List<ParentStudentDto>> GetMyStudentsAsync(string parentUserId);
    Task<StudentLearningOverviewDto?> GetStudentLearningOverviewAsync(string parentUserId, int studentId);
}
