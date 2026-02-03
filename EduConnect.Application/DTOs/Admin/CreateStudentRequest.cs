namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Add student (P1–P4) to parent — Master Doc B3.
/// </summary>
public class CreateStudentRequest
{
    public string ParentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int GradeLevel { get; set; } // 1=P1 .. 4=P4
    public DateTime DateOfBirth { get; set; }
    public string? SpecialNeeds { get; set; }
}
