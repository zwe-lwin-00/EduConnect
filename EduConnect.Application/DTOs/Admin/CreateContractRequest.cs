namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Admin creates 1:1 class: assign teacher and student. Either SubscriptionId (parent-paid) or StartDate (legacy monthly).
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
}
