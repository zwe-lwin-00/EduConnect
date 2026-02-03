namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Contract = financial heart — Master Doc B4, 6.2.
/// </summary>
public class ContractDto
{
    public int Id { get; set; }
    public string ContractId { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int PackageHours { get; set; }
    public int RemainingHours { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
