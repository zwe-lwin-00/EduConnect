namespace EduConnect.Domain.Entities;

public class TeacherAvailability
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public TeacherProfile Teacher { get; set; } = null!;
}
