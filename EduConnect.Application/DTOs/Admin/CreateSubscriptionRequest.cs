using EduConnect.Shared.Enums;

namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Parent-paid subscription: student, type (one-to-one or group), and duration in months.
/// </summary>
public class CreateSubscriptionRequest
{
    public int StudentId { get; set; }
    public SubscriptionType Type { get; set; }
    /// <summary>Number of months (e.g. 1 or 2). Period runs from start of current month (or StartDate if provided).</summary>
    public int DurationMonths { get; set; } = 1;
    /// <summary>Optional: start date; if not set, first day of current month (UTC).</summary>
    public DateTime? StartDate { get; set; }
}
