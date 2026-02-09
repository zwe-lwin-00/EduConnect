using EduConnect.Application.Common.Interfaces;
using EduConnect.Application.DTOs.GroupClass;

namespace EduConnect.Application.Features.GroupClass.Interfaces;

public interface IGroupClassService : IService
{
    Task<GroupClassDto> CreateAsync(int teacherId, string name, string? zoomJoinUrl = null);
    Task<List<GroupClassDto>> GetByTeacherAsync(int teacherId);
    Task<GroupClassDto?> GetByIdAsync(int groupClassId, int teacherId);
    Task<bool> UpdateAsync(int groupClassId, int teacherId, string name, bool isActive, string? zoomJoinUrl = null);
    Task<bool> EnrollStudentAsync(int groupClassId, int teacherId, int studentId, int contractId);
    Task<bool> UnenrollAsync(int enrollmentId, int teacherId);
    Task<List<GroupClassEnrollmentDto>> GetEnrollmentsAsync(int groupClassId, int teacherId);
}
