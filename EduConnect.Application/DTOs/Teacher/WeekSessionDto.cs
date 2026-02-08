namespace EduConnect.Application.DTOs.Teacher;

/// <summary>
/// A session in a week view (from attendance logs). Used for teacher "My sessions this week" and parent "Upcoming sessions" for a student.
/// </summary>
public class WeekSessionDto
{
    public int AttendanceLogId { get; set; }
    public int ContractId { get; set; }
    public string ContractIdDisplay { get; set; } = string.Empty;
    /// <summary>Date of the session (date only, in app timezone).</summary>
    public DateTime Date { get; set; }
    /// <summary>Start time as "HH:mm".</summary>
    public string StartTime { get; set; } = string.Empty;
    /// <summary>End time as "HH:mm" if checked out.</summary>
    public string? EndTime { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal HoursUsed { get; set; }
}
