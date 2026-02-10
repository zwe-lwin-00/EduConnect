using EduConnect.Shared.Enums;

namespace EduConnect.Domain.Entities;

public class ContractSession
{
    public int Id { get; set; }
    public string ContractId { get; set; } = string.Empty; // Unique identifier
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    public int PackageHours { get; set; }  // Kept for DB; not used (monthly-only)
    public int RemainingHours { get; set; } // Kept for DB; not used (monthly-only)
    public BillingType BillingType { get; set; } = BillingType.Monthly; // Kept for DB; not used
    /// <summary>Subscription valid from 1st of month to this date (end of month). Access when UtcNow &lt;= this.</summary>
    public DateTime? SubscriptionPeriodEnd { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Active;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; } // Admin UserId

    /// <summary>True if contract allows attending sessions: active and subscription period (1st–last of month) includes today.</summary>
    public bool HasActiveAccess()
    {
        return Status == ContractStatus.Active
            && SubscriptionPeriodEnd.HasValue
            && SubscriptionPeriodEnd.Value >= DateTime.UtcNow;
    }

    // Navigation properties
    public TeacherProfile Teacher { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
    public ICollection<Homework> Homeworks { get; set; } = new List<Homework>();
    public ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();
}
