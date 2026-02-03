using EduConnect.Application.Common.Interfaces;

namespace EduConnect.Application.Features.Students.Interfaces;

public interface IStudentService : IService
{
    Task<int> CreateStudentAsync(CreateStudentDto dto);
    Task<bool> UpdateStudentAsync(int studentId, UpdateStudentDto dto);
    Task<StudentDto> GetStudentByIdAsync(int studentId);
    Task<List<StudentDto>> GetStudentsByParentAsync(string parentId);
}

public class CreateStudentDto
{
    public string ParentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? SpecialNeeds { get; set; }
}

public class UpdateStudentDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public string? SpecialNeeds { get; set; }
}

public class StudentDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? SpecialNeeds { get; set; }
    public decimal WalletBalance { get; set; }
}
