namespace EduConnect.Application.DTOs.Teacher;

/// <summary>
/// Request from client — times as strings (e.g. "09:00") for JSON compatibility.
/// </summary>
public class TeacherAvailabilityRequestDto
{
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}
