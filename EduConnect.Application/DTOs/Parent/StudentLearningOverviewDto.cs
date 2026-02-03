namespace EduConnect.Application.DTOs.Parent;

/// <summary>
/// Student learning overview (read-only) — Master Doc 11C C1. Assigned teacher, subjects, upcoming, completed. No edits.
/// </summary>
public class StudentLearningOverviewDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public List<AssignedTeacherDto> AssignedTeachers { get; set; } = new();
    public string Subjects { get; set; } = string.Empty;
    public int TotalRemainingHours { get; set; }
    public List<UpcomingSessionDto> UpcomingSessions { get; set; } = new();
    public List<CompletedSessionDto> CompletedSessions { get; set; } = new();
}

public class AssignedTeacherDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string ContractIdDisplay { get; set; } = string.Empty;
    public int RemainingHours { get; set; }
}

public class UpcomingSessionDto
{
    public string ContractIdDisplay { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int RemainingHours { get; set; }
}

/// <summary>
/// Completed session — read-only. Lesson notes / progress (AI summary). Master Doc 11C C3.
/// </summary>
public class CompletedSessionDto
{
    public int SessionId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal HoursUsed { get; set; }
    public string? LessonNotes { get; set; }
    public string? ProgressReport { get; set; }
    public string TeacherName { get; set; } = string.Empty;
}
