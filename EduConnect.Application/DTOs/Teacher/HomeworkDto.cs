using EduConnect.Shared.Enums;

namespace EduConnect.Application.DTOs.Teacher;

public class HomeworkDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int? ContractSessionId { get; set; }
    public string? ContractIdDisplay { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public HomeworkStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
    public string? TeacherFeedback { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateHomeworkRequest
{
    public int StudentId { get; set; }
    public int? ContractSessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
}

public class UpdateHomeworkStatusRequest
{
    public HomeworkStatus Status { get; set; }
    public string? TeacherFeedback { get; set; }
}
