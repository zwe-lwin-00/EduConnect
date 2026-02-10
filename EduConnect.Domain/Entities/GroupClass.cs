namespace EduConnect.Domain.Entities;

/// <summary>
/// A group class: one teacher, multiple students. Admin sets schedule (days, start/end time); teacher adds Zoom link.
/// </summary>
public class GroupClass
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>Comma-separated ISO day numbers (1=Monday .. 7=Sunday), e.g. "1,3,5".</summary>
    public string? DaysOfWeek { get; set; }
    /// <summary>Class start time (local/app timezone, e.g. Myanmar).</summary>
    public TimeOnly? StartTime { get; set; }
    /// <summary>Class end time (local/app timezone).</summary>
    public TimeOnly? EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>Zoom meeting join URL — set by assigned teacher, not admin.</summary>
    public string? ZoomJoinUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TeacherProfile Teacher { get; set; } = null!;
    public ICollection<GroupClassEnrollment> Enrollments { get; set; } = new List<GroupClassEnrollment>();
    public ICollection<GroupSession> Sessions { get; set; } = new List<GroupSession>();
}
