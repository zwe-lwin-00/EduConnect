using EduConnect.Application.Common.Interfaces;
using EduConnect.Application.DTOs.Teacher;

namespace EduConnect.Application.Features.Teachers.Interfaces;

public interface ITeacherService : IService
{
    Task<int?> GetTeacherIdByUserIdAsync(string userId);
    Task<TeacherDashboardDto> GetDashboardAsync(int teacherId);
    Task<TeacherProfileDto> GetProfileAsync(int teacherId);
    Task UpdateZoomJoinUrlAsync(int teacherId, string? zoomJoinUrl);
    Task<List<TeacherAssignedStudentDto>> GetAssignedStudentsAsync(int teacherId);
    Task<List<TeacherSessionItemDto>> GetTodaySessionsAsync(int teacherId);
    Task<List<TeacherSessionItemDto>> GetUpcomingSessionsAsync(int teacherId);
    Task<List<WeekSessionDto>> GetSessionsForWeekAsync(int teacherId, DateTime weekStartMonday);
    Task UpdateAvailabilityAsync(int teacherId, List<TeacherAvailabilityDto> availabilities);
    Task UpdateAvailabilityFromRequestAsync(int teacherId, List<TeacherAvailabilityRequestDto> availabilities);
    Task<List<TeacherAvailabilityDto>> GetAvailabilityAsync(int teacherId);
}

public class TeacherAvailabilityDto
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
