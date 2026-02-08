using EduConnect.Shared.Enums;

namespace EduConnect.Domain.Entities;

public class Homework
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int? ContractSessionId { get; set; } // Optional link to contract/subject
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public HomeworkStatus Status { get; set; } = HomeworkStatus.Assigned;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
    public string? TeacherFeedback { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Student Student { get; set; } = null!;
    public TeacherProfile Teacher { get; set; } = null!;
    public ContractSession? ContractSession { get; set; } // Optional: which contract/subject
}
