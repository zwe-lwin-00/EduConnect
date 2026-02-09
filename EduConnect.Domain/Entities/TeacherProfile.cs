using EduConnect.Shared.Enums;

namespace EduConnect.Domain.Entities;

public class TeacherProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string NrcNumber { get; set; } = string.Empty; // Encrypted
    public string EducationLevel { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    public string? CertificateUrl { get; set; }
    public string? NrcDocumentUrl { get; set; }
    public string? Bio { get; set; }
    public string? Specializations { get; set; } // Comma-separated subjects
    /// <summary>Default Zoom meeting join URL for 1:1 sessions (e.g. teacher's personal meeting room).</summary>
    public string? ZoomJoinUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ContractSession> ContractSessions { get; set; } = new List<ContractSession>();
    public ICollection<TeacherAvailability> Availabilities { get; set; } = new List<TeacherAvailability>();
    public ICollection<Homework> Homeworks { get; set; } = new List<Homework>();
    public ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();
    public ICollection<GroupClass> GroupClasses { get; set; } = new List<GroupClass>();
}
