namespace EduConnect.Application.DTOs.Teacher;

/// <summary>
/// Assigned students view — Master Doc 11B B3. Student name, grade, subjects, contract status. No parent contact.
/// </summary>
public class TeacherAssignedStudentDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string Subjects { get; set; } = string.Empty;
    public string ContractStatus { get; set; } = string.Empty;
    /// <summary>ContractSession.Id — use for group class enrollment.</summary>
    public int ContractId { get; set; }
    public string ContractIdDisplay { get; set; } = string.Empty;
    public int RemainingHours { get; set; }
}
