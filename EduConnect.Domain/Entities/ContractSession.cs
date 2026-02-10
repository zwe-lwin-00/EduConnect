using EduConnect.Shared.Enums;

namespace EduConnect.Domain.Entities;

public class ContractSession
{
    public int Id { get; set; }
    public string ContractId { get; set; } = string.Empty; // Unique identifier
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    /// <summary>When set, this 1:1 class is funded by this subscription (parent-paid). Access = subscription active.</summary>
    public int? SubscriptionId { get; set; }
    public int PackageHours { get; set; }  // Kept for DB; not used (monthly-only)
    public int RemainingHours { get; set; } // Kept for DB; not used (monthly-only)
    public BillingType BillingType { get; set; } = BillingType.Monthly; // Kept for DB; not used
    /// <summary>Subscription valid from 1st of month to this date (end of month). Access when UtcNow &lt;= this.</summary>
    public DateTime? SubscriptionPeriodEnd { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Active;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    /// <summary>Comma-separated ISO day numbers (1=Monday .. 7=Sunday), e.g. "1,3,5".</summary>
    public string? DaysOfWeek { get; set; }
    /// <summary>Class start time (local/app timezone).</summary>
    public TimeOnly? StartTime { get; set; }
    /// <summary>Class end time (local/app timezone).</summary>
    public TimeOnly? EndTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; } // Admin UserId

    /// <summary>True if this 1:1 class allows attending sessions: when SubscriptionId is set, uses Subscription; else legacy SubscriptionPeriodEnd.</summary>
    public bool HasActiveAccess()
    {
        if (Status != ContractStatus.Active) return false;
        if (SubscriptionId.HasValue && Subscription != null)
            return Subscription.HasActiveAccess();
        return SubscriptionPeriodEnd.HasValue && SubscriptionPeriodEnd.Value >= DateTime.UtcNow;
    }

    // Navigation properties
    public TeacherProfile Teacher { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Subscription? Subscription { get; set; }
    public ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
    public ICollection<Homework> Homeworks { get; set; } = new List<Homework>();
    public ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();
}
