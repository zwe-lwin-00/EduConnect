using EduConnect.Shared.Enums;

namespace EduConnect.Domain.Entities;

/// <summary>
/// One delivered group class session (teacher checked in and out for a group class).
/// </summary>
public class GroupSession
{
    public int Id { get; set; }
    public int GroupClassId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal TotalDurationHours { get; set; }
    public string? LessonNotes { get; set; }
    /// <summary>Zoom join URL for this session (if null, use group class ZoomJoinUrl).</summary>
    public string? ZoomJoinUrl { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.InProgress;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public GroupClass GroupClass { get; set; } = null!;
    public ICollection<GroupSessionAttendance> Attendances { get; set; } = new List<GroupSessionAttendance>();
}
