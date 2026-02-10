namespace EduConnect.Domain.Entities;

/// <summary>
/// Links a student to a group class. ContractId is the 1:1 contract; subscription validity is checked when the student attends a group session.
/// </summary>
public class GroupClassEnrollment
{
    public int Id { get; set; }
    public int GroupClassId { get; set; }
    public int StudentId { get; set; }
    /// <summary>Legacy: 1:1 contract used for enrollment. When SubscriptionId is set, group access uses subscription.</summary>
    public int? ContractId { get; set; }
    /// <summary>When set, this enrollment is funded by this subscription (parent-paid group). Access = subscription active.</summary>
    public int? SubscriptionId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public GroupClass GroupClass { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public ContractSession? ContractSession { get; set; }
    public Subscription? Subscription { get; set; }
}
