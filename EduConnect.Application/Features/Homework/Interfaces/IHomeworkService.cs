using EduConnect.Application.Common.Interfaces;
using EduConnect.Application.DTOs.Teacher;

namespace EduConnect.Application.Features.Homework.Interfaces;

public interface IHomeworkService : IService
{
    Task<HomeworkDto> CreateHomeworkAsync(int teacherId, CreateHomeworkRequest request);
    Task<List<HomeworkDto>> GetHomeworksByTeacherAsync(int teacherId, int? studentId = null, DateTime? dueDateFrom = null, DateTime? dueDateTo = null);
    Task<List<HomeworkDto>> GetHomeworksByStudentAsync(int studentId);
    Task<HomeworkDto?> GetHomeworkByIdAsync(int homeworkId);
    Task<HomeworkDto?> UpdateHomeworkStatusAsync(int teacherId, int homeworkId, UpdateHomeworkStatusRequest request);
    Task<bool> TeacherCanAccessStudentAsync(int teacherId, int studentId);

    Task<StudentGradeDto> CreateGradeAsync(int teacherId, CreateGradeRequest request);
    Task<List<StudentGradeDto>> GetGradesByTeacherAsync(int teacherId, int? studentId = null, DateTime? gradeDateFrom = null, DateTime? gradeDateTo = null);
    Task<List<StudentGradeDto>> GetGradesByStudentAsync(int studentId);
}
