namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Admin creates 1:1 class: assign teacher, student, schedule (days, start/end time). Either SubscriptionId (parent-paid) or StartDate (legacy monthly).
/// </summary>
public class CreateContractRequest
{
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    /// <summary>When set, this 1:1 class is funded by this subscription (OneToOne type). Period comes from subscription.</summary>
    public int? SubscriptionId { get; set; }
    /// <summary>When SubscriptionId is null: subscription period is the calendar month containing this date.</summary>
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    /// <summary>Comma-separated ISO day numbers (1=Monday .. 7=Sunday), e.g. "1,3,5".</summary>
    public string? DaysOfWeek { get; set; }
    /// <summary>Class start time (e.g. "09:00" in local timezone).</summary>
    public string? StartTime { get; set; }
    /// <summary>Class end time (e.g. "10:00" in local timezone).</summary>
    public string? EndTime { get; set; }
}
