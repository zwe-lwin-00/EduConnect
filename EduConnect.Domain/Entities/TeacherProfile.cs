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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ContractSession> ContractSessions { get; set; } = new List<ContractSession>();
    public ICollection<TeacherAvailability> Availabilities { get; set; } = new List<TeacherAvailability>();
}
