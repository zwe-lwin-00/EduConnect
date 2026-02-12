using EduConnect.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduConnect.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool MustChangePassword { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public TeacherProfile? TeacherProfile { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
