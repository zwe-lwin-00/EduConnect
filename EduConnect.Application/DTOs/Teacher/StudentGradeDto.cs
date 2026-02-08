namespace EduConnect.Application.DTOs.Teacher;

public class StudentGradeDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int? ContractSessionId { get; set; }
    public string? ContractIdDisplay { get; set; }
    public string Title { get; set; } = string.Empty;
    public string GradeValue { get; set; } = string.Empty;
    public decimal? MaxValue { get; set; }
    public DateTime GradeDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateGradeRequest
{
    public int StudentId { get; set; }
    public int? ContractSessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string GradeValue { get; set; } = string.Empty;
    public decimal? MaxValue { get; set; }
    public DateTime GradeDate { get; set; }
    public string? Notes { get; set; }
}
