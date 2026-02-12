namespace EduConnect.Application.DTOs.GroupClass;

public class GroupClassDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    /// <summary>Set when returned for admin (list/detail).</summary>
    public string? TeacherName { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>Comma-separated ISO day numbers (1=Monday .. 7=Sunday).</summary>
    public string? DaysOfWeek { get; set; }
    /// <summary>Class start time (e.g. "09:00").</summary>
    public string? StartTime { get; set; }
    /// <summary>Class end time (e.g. "10:00").</summary>
    public string? EndTime { get; set; }
    /// <summary>Optional: class runs from this date (ISO date).</summary>
    public DateTime? StartDate { get; set; }
    /// <summary>Optional: class runs until this date (ISO date).</summary>
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    /// <summary>Zoom meeting join URL — set by teacher.</summary>
    public string? ZoomJoinUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EnrolledCount { get; set; }
}

public class GroupClassEnrollmentDto
{
    public int Id { get; set; }
    public int GroupClassId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int? ContractId { get; set; }
    public string? ContractIdDisplay { get; set; }
    public int? SubscriptionId { get; set; }
    public string? SubscriptionIdDisplay { get; set; }
}

public class GroupSessionDto
{
    public int Id { get; set; }
    public int GroupClassId { get; set; }
    public string GroupClassName { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal TotalDurationHours { get; set; }
    public string? LessonNotes { get; set; }
    /// <summary>Zoom meeting join URL for this session (group class default or session override).</summary>
    public string? ZoomJoinUrl { get; set; }
    public int Status { get; set; }
    public int AttendeeCount { get; set; }
}
