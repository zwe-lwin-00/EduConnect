using EduConnect.Shared.Enums;

namespace EduConnect.Domain.Entities;

public class AttendanceLog
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int SessionId { get; set; } // Unique session identifier
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal HoursUsed { get; set; }
    public string? LessonNotes { get; set; }
    /// <summary>Zoom join URL for this session (if null, use teacher's default ZoomJoinUrl).</summary>
    public string? ZoomJoinUrl { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;
    public string? ProgressReport { get; set; } // AI-generated summary
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ContractSession ContractSession { get; set; } = null!;
}
