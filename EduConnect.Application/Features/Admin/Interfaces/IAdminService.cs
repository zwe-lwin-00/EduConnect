using EduConnect.Application.Common.Interfaces;
using EduConnect.Application.Common.Models;
using EduConnect.Application.DTOs.Admin;

namespace EduConnect.Application.Features.Admin.Interfaces;

public interface IAdminService : IService
{
    // Dashboard — Master Doc B1
    Task<DashboardDto> GetDashboardAsync();

    // Teachers — Master Doc B2
    Task<OnboardTeacherResponse> OnboardTeacherAsync(OnboardTeacherRequest request, string adminUserId);
    Task<ResetTeacherPasswordResponse> ResetTeacherPasswordAsync(int teacherId);
    Task<bool> VerifyTeacherAsync(int teacherId);
    Task<bool> RejectTeacherAsync(int teacherId, string reason);
    Task<bool> SetTeacherActiveAsync(int teacherId, bool isActive);
    Task<List<TeacherDto>> GetTeachersAsync(string? searchTerm = null, int? verificationStatus = null, string? specializations = null);
    Task<TeacherDto?> GetTeacherByIdAsync(int teacherId);
    Task<bool> UpdateTeacherAsync(int teacherId, UpdateTeacherRequest request);
    Task<PagedResult<TeacherDto>> GetTeachersPagedAsync(PagedRequest request, int? verificationStatus = null, string? specializations = null);

    // Parents & Students — Master Doc B3
    Task<CreateParentResponse> CreateParentAsync(CreateParentRequest request, string adminUserId);
    Task<int> CreateStudentAsync(CreateStudentRequest request, string adminUserId);
    Task<List<ParentDto>> GetParentsAsync();
    Task<ParentDto?> GetParentByIdAsync(string parentId);
    Task<List<StudentDto>> GetStudentsAsync(string? parentId = null, int? gradeLevel = null);
    Task<List<StudentDto>> GetStudentsByParentAsync(string parentId);
    Task<PagedResult<ParentDto>> GetParentsPagedAsync(PagedRequest request);

    // Contracts — Master Doc B4
    Task<List<ContractDto>> GetContractsAsync(int? teacherId = null, int? studentId = null, int? status = null);
    Task<ContractDto?> GetContractByIdAsync(int id);
    Task<ContractDto> CreateContractAsync(CreateContractRequest request, string adminUserId);
    Task<bool> ActivateContractAsync(int id);
    Task<bool> CancelContractAsync(int id);

    // Attendance oversight — Master Doc B6
    Task<List<TodaySessionDto>> GetTodaySessionsAsync();
    Task<bool> OverrideCheckInAsync(int attendanceLogId, string adminUserId);
    Task<bool> OverrideCheckOutAsync(int attendanceLogId, string adminUserId);
    Task<bool> AdjustSessionHoursAsync(int attendanceLogId, AdjustHoursRequest request, string adminUserId);

    // Subscriptions — Master Doc B7 (monthly only)
    Task<bool> RenewSubscriptionAsync(int contractId, string adminUserId);
    Task<bool> SetStudentActiveAsync(int studentId, bool isActive);

    // Reports — Master Doc B8
    Task<DailyReportDto> GetDailyReportAsync(DateTime date);
    Task<MonthlyReportDto> GetMonthlyReportAsync(int year, int month);
}
