using EduConnect.Shared.Enums;

namespace EduConnect.Domain.Entities;

public class Student
{
    public int Id { get; set; }
    public string ParentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public GradeLevel GradeLevel { get; set; }
    public string? SpecialNeeds { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser Parent { get; set; } = null!;
    public ICollection<ContractSession> ContractSessions { get; set; } = new List<ContractSession>();
}
