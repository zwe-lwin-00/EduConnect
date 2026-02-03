using EduConnect.Shared.Enums;

namespace EduConnect.Domain.Entities;

public class ContractSession
{
    public int Id { get; set; }
    public string ContractId { get; set; } = string.Empty; // Unique identifier
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    public int PackageHours { get; set; }
    public int RemainingHours { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Active;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; } // Admin UserId
    
    // Navigation properties
    public TeacherProfile Teacher { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
}
