namespace EduConnect.Application.DTOs.Admin;

/// <summary>Admin creates a group class: name, teacher, schedule (days, start/end time). Zoom is set by teacher.</summary>
public class AdminCreateGroupClassRequest
{
    public string Name { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    /// <summary>Comma-separated ISO day numbers (1=Monday .. 7=Sunday), e.g. "1,3,5".</summary>
    public string? DaysOfWeek { get; set; }
    /// <summary>Class start time (e.g. "09:00" in local timezone).</summary>
    public string? StartTime { get; set; }
    /// <summary>Class end time (e.g. "10:00" in local timezone).</summary>
    public string? EndTime { get; set; }
    /// <summary>Optional: class runs from this date (UTC date). When set, enrollment requires subscription to cover this period.</summary>
    public DateTime? StartDate { get; set; }
    /// <summary>Optional: class runs until this date (UTC date). When set, enrollment requires subscription to cover this period.</summary>
    public DateTime? EndDate { get; set; }
}

/// <summary>Admin can update name, teacher, active, and schedule. Zoom is set by teacher only.</summary>
public class AdminUpdateGroupClassRequest
{
    public string? Name { get; set; }
    public int TeacherId { get; set; }
    public bool IsActive { get; set; }
    public string? DaysOfWeek { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
