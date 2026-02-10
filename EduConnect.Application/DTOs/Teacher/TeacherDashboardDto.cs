namespace EduConnect.Application.DTOs.Teacher;

/// <summary>
/// Teacher dashboard — Master Doc 11B B1. Today's Sessions, Upcoming (subscription-based), Alerts. No revenue, no analytics.
/// </summary>
public class TeacherDashboardDto
{
    public List<TeacherSessionItemDto> TodaySessions { get; set; } = new();
    public List<TeacherSessionItemDto> UpcomingSessions { get; set; } = new();
    /// <summary>Legacy; always 0 (monthly subscription only).</summary>
    public int TotalRemainingHours { get; set; }
    public List<TeacherAlertDto> Alerts { get; set; } = new();
}

public class TeacherSessionItemDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string ContractIdDisplay { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledTime { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? LessonNotes { get; set; }
    public bool CanCheckIn { get; set; }
    public bool CanCheckOut { get; set; }
    /// <summary>Zoom meeting join URL for this session (teacher's default or session override).</summary>
    public string? ZoomJoinUrl { get; set; }
}

public class TeacherAlertDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
