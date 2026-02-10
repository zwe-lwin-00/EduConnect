using EduConnect.Application.Common.Interfaces;
using EduConnect.Application.DTOs.Admin;
using EduConnect.Application.DTOs.GroupClass;

namespace EduConnect.Application.Features.GroupClass.Interfaces;

public interface IGroupClassService : IService
{
    Task<GroupClassDto> CreateAsync(int teacherId, string name, string? zoomJoinUrl = null);
    /// <summary>Admin: create group class with schedule (days, start/end time). Zoom is set by teacher.</summary>
    Task<GroupClassDto> CreateByAdminAsync(AdminCreateGroupClassRequest request);
    Task<List<GroupClassDto>> GetByTeacherAsync(int teacherId);
    Task<GroupClassDto?> GetByIdAsync(int groupClassId, int teacherId);
    Task<bool> UpdateAsync(int groupClassId, int teacherId, string name, bool isActive, string? zoomJoinUrl = null);
    Task<bool> EnrollStudentAsync(int groupClassId, int teacherId, int studentId, int contractId);
    /// <summary>Admin: enroll student in group class using their Group subscription (no contract).</summary>
    Task<bool> EnrollStudentBySubscriptionAsync(int groupClassId, int studentId, int subscriptionId);
    Task<bool> UnenrollAsync(int enrollmentId, int teacherId);
    Task<List<GroupClassEnrollmentDto>> GetEnrollmentsAsync(int groupClassId, int teacherId);

    /// <summary>Admin: list all group classes with teacher name.</summary>
    Task<List<GroupClassDto>> GetAllForAdminAsync();
    /// <summary>Admin: get any group class by id (with teacher name).</summary>
    Task<GroupClassDto?> GetByIdForAdminAsync(int groupClassId);
    /// <summary>Admin: update group class (teacher, name, active, schedule). Zoom is set by teacher only.</summary>
    Task<bool> UpdateByAdminAsync(int groupClassId, AdminUpdateGroupClassRequest request);
    /// <summary>Admin: get enrollments for any group class.</summary>
    Task<List<GroupClassEnrollmentDto>> GetEnrollmentsForAdminAsync(int groupClassId);
    /// <summary>Admin: remove enrollment (any group class).</summary>
    Task<bool> UnenrollByAdminAsync(int enrollmentId);
}
