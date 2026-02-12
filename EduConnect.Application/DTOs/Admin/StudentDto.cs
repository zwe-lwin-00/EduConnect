namespace EduConnect.Application.DTOs.Admin;

public class StudentDto
{
    public int Id { get; set; }
    public string ParentId { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public string GradeLevelName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? SpecialNeeds { get; set; }
    public decimal WalletBalance { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
