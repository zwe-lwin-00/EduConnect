using EduConnect.Shared.Enums;

namespace EduConnect.Domain.Entities;

/// <summary>
/// Parent-paid subscription for a student: duration (e.g. 1 or 2 months) and type (one-to-one or group).
/// Admin creates classes and assigns teacher + student(s); the subscription entitles the student to be in that class.
/// </summary>
public class Subscription
{
    public int Id { get; set; }
    public string SubscriptionId { get; set; } = string.Empty; // e.g. SUB-XXXXXXXX
    public int StudentId { get; set; }
    public SubscriptionType Type { get; set; }
    public DateTime StartDate { get; set; }
    /// <summary>Subscription valid until this date (inclusive).</summary>
    public DateTime SubscriptionPeriodEnd { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    public Student Student { get; set; } = null!;
    public ICollection<ContractSession> ContractSessions { get; set; } = new List<ContractSession>();
    public ICollection<GroupClassEnrollment> GroupClassEnrollments { get; set; } = new List<GroupClassEnrollment>();

    public bool HasActiveAccess()
    {
        return Status == ContractStatus.Active && SubscriptionPeriodEnd.Date >= DateTime.UtcNow.Date;
    }
}
