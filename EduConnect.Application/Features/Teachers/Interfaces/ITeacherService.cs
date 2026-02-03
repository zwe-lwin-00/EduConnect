using EduConnect.Application.Common.Interfaces;

namespace EduConnect.Application.Features.Teachers.Interfaces;

public interface ITeacherService : IService
{
    Task UpdateAvailabilityAsync(int teacherId, List<TeacherAvailabilityDto> availabilities);
    Task<List<TeacherAvailabilityDto>> GetAvailabilityAsync(int teacherId);
}

public class TeacherAvailabilityDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
