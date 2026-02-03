using EduConnect.Application.Common.Interfaces;
using EduConnect.Application.DTOs.Admin;

namespace EduConnect.Application.Features.Admin.Interfaces;

public interface IAdminService : IService
{
    Task<string> OnboardTeacherAsync(OnboardTeacherRequest request);
    Task<bool> VerifyTeacherAsync(int teacherId);
    Task<bool> RejectTeacherAsync(int teacherId, string reason);
}
