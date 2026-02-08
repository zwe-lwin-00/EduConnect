namespace EduConnect.Domain.Entities;

public class StudentGrade
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int? ContractSessionId { get; set; } // Optional link to contract/subject
    public string Title { get; set; } = string.Empty; // e.g. "Math Quiz 1", "Mid-term"
    public string GradeValue { get; set; } = string.Empty; // e.g. "A", "85", "Pass"
    public decimal? MaxValue { get; set; } // e.g. 100 for percentage
    public DateTime GradeDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Student Student { get; set; } = null!;
    public TeacherProfile Teacher { get; set; } = null!;
    public ContractSession? ContractSession { get; set; } // Optional: which contract/subject
}
