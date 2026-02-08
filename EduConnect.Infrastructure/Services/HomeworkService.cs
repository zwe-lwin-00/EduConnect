using EduConnect.Application.Common.Exceptions;
using EduConnect.Application.DTOs.Parent;
using EduConnect.Application.DTOs.Teacher;
using EduConnect.Application.Features.Homework.Interfaces;
using EduConnect.Application.Features.Notifications.Interfaces;
using EduConnect.Domain.Entities;
using EduConnect.Infrastructure.Data;
using EduConnect.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastructure.Services;

public class HomeworkService : IHomeworkService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public HomeworkService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<bool> TeacherCanAccessStudentAsync(int teacherId, int studentId)
    {
        return await _context.ContractSessions
            .AnyAsync(c => c.TeacherId == teacherId && c.StudentId == studentId && c.Status == ContractStatus.Active);
    }

    public async Task<HomeworkDto> CreateHomeworkAsync(int teacherId, CreateHomeworkRequest request)
    {
        if (!await TeacherCanAccessStudentAsync(teacherId, request.StudentId))
            throw new BusinessException("You can only assign homework to students you have an active contract with.");

        var student = await _context.Students.FindAsync(request.StudentId);
        var teacher = await _context.TeacherProfiles.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == teacherId);
        if (student == null) throw new NotFoundException("Student", request.StudentId);
        if (teacher == null) throw new NotFoundException("Teacher", teacherId);

        ContractSession? contract = null;
        if (request.ContractSessionId.HasValue)
        {
            contract = await _context.ContractSessions
                .FirstOrDefaultAsync(c => c.Id == request.ContractSessionId.Value && c.TeacherId == teacherId && c.StudentId == request.StudentId);
        }

        var homework = new Homework
        {
            StudentId = request.StudentId,
            TeacherId = teacherId,
            ContractSessionId = request.ContractSessionId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(request.DueDate, DateTimeKind.Utc) : request.DueDate.ToUniversalTime(),
            Status = HomeworkStatus.Assigned
        };
        _context.Homeworks.Add(homework);
        await _context.SaveChangesAsync();

        var studentName = $"{student.FirstName} {student.LastName}";
        await _notificationService.CreateForUserAsync(student.ParentId, "New homework for " + studentName, $"{request.Title}. Due {request.DueDate:dd MMM yyyy}.", NotificationType.HomeworkAssigned, "Homework", homework.Id);

        return MapToHomeworkDto(homework, student, teacher, contract);
    }

    public async Task<List<HomeworkDto>> GetHomeworksByTeacherAsync(int teacherId, int? studentId = null, DateTime? dueDateFrom = null, DateTime? dueDateTo = null)
    {
        var query = _context.Homeworks
            .Include(h => h.Student)
            .Include(h => h.Teacher).ThenInclude(t => t!.User)
            .Include(h => h.ContractSession)
            .Where(h => h.TeacherId == teacherId);
        if (studentId.HasValue)
            query = query.Where(h => h.StudentId == studentId.Value);
        if (dueDateFrom.HasValue)
            query = query.Where(h => h.DueDate.Date >= dueDateFrom.Value.Date);
        if (dueDateTo.HasValue)
            query = query.Where(h => h.DueDate.Date <= dueDateTo.Value.Date);
        var list = await query.OrderByDescending(h => h.CreatedAt).ToListAsync();
        return list.Select(h => MapToHomeworkDto(h, h.Student, h.Teacher!, h.ContractSession)).ToList();
    }

    public async Task<List<HomeworkDto>> GetHomeworksByStudentAsync(int studentId)
    {
        var list = await _context.Homeworks
            .Include(h => h.Student)
            .Include(h => h.Teacher).ThenInclude(t => t!.User)
            .Include(h => h.ContractSession)
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.DueDate)
            .ToListAsync();
        return list.Select(h => MapToHomeworkDto(h, h.Student, h.Teacher!, h.ContractSession)).ToList();
    }

    public async Task<HomeworkDto?> GetHomeworkByIdAsync(int homeworkId)
    {
        var h = await _context.Homeworks
            .Include(x => x.Student)
            .Include(x => x.Teacher).ThenInclude(t => t!.User)
            .Include(x => x.ContractSession)
            .FirstOrDefaultAsync(x => x.Id == homeworkId);
        return h == null ? null : MapToHomeworkDto(h, h.Student, h.Teacher!, h.ContractSession);
    }

    public async Task<HomeworkDto?> UpdateHomeworkStatusAsync(int teacherId, int homeworkId, UpdateHomeworkStatusRequest request)
    {
        var homework = await _context.Homeworks
            .Include(h => h.Student)
            .Include(h => h.Teacher).ThenInclude(t => t!.User)
            .Include(h => h.ContractSession)
            .FirstOrDefaultAsync(h => h.Id == homeworkId && h.TeacherId == teacherId);
        if (homework == null) return null;

        homework.Status = request.Status;
        homework.TeacherFeedback = request.TeacherFeedback;
        homework.UpdatedAt = DateTime.UtcNow;
        if (request.Status == HomeworkStatus.Submitted)
            homework.SubmittedAt = DateTime.UtcNow;
        else if (request.Status == HomeworkStatus.Graded)
            homework.GradedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToHomeworkDto(homework, homework.Student, homework.Teacher!, homework.ContractSession);
    }

    public async Task<StudentGradeDto> CreateGradeAsync(int teacherId, CreateGradeRequest request)
    {
        if (!await TeacherCanAccessStudentAsync(teacherId, request.StudentId))
            throw new BusinessException("You can only add grades for students you have an active contract with.");

        var student = await _context.Students.FindAsync(request.StudentId);
        var teacher = await _context.TeacherProfiles.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == teacherId);
        if (student == null) throw new NotFoundException("Student", request.StudentId);
        if (teacher == null) throw new NotFoundException("Teacher", teacherId);

        ContractSession? contract = null;
        if (request.ContractSessionId.HasValue)
        {
            contract = await _context.ContractSessions
                .FirstOrDefaultAsync(c => c.Id == request.ContractSessionId.Value && c.TeacherId == teacherId && c.StudentId == request.StudentId);
        }

        var grade = new StudentGrade
        {
            StudentId = request.StudentId,
            TeacherId = teacherId,
            ContractSessionId = request.ContractSessionId,
            Title = request.Title,
            GradeValue = request.GradeValue,
            MaxValue = request.MaxValue,
            GradeDate = request.GradeDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(request.GradeDate, DateTimeKind.Utc) : request.GradeDate.ToUniversalTime(),
            Notes = request.Notes
        };
        _context.StudentGrades.Add(grade);
        await _context.SaveChangesAsync();

        var studentName = $"{student.FirstName} {student.LastName}";
        await _notificationService.CreateForUserAsync(student.ParentId, "New grade recorded", $"{request.Title}: {request.GradeValue}" + (request.MaxValue.HasValue ? $"/{request.MaxValue}" : "") + $" for {studentName}.", NotificationType.GradeRecorded, "Grade", grade.Id);

        return MapToGradeDto(grade, student, teacher, contract);
    }

    public async Task<List<StudentGradeDto>> GetGradesByTeacherAsync(int teacherId, int? studentId = null, DateTime? gradeDateFrom = null, DateTime? gradeDateTo = null)
    {
        var query = _context.StudentGrades
            .Include(g => g.Student)
            .Include(g => g.Teacher).ThenInclude(t => t!.User)
            .Include(g => g.ContractSession)
            .Where(g => g.TeacherId == teacherId);
        if (studentId.HasValue)
            query = query.Where(g => g.StudentId == studentId.Value);
        if (gradeDateFrom.HasValue)
            query = query.Where(g => g.GradeDate.Date >= gradeDateFrom.Value.Date);
        if (gradeDateTo.HasValue)
            query = query.Where(g => g.GradeDate.Date <= gradeDateTo.Value.Date);
        var list = await query.OrderByDescending(g => g.GradeDate).ToListAsync();
        return list.Select(g => MapToGradeDto(g, g.Student, g.Teacher!, g.ContractSession)).ToList();
    }

    public async Task<List<StudentGradeDto>> GetGradesByStudentAsync(int studentId)
    {
        var list = await _context.StudentGrades
            .Include(g => g.Student)
            .Include(g => g.Teacher).ThenInclude(t => t!.User)
            .Include(g => g.ContractSession)
            .Where(g => g.StudentId == studentId)
            .OrderByDescending(g => g.GradeDate)
            .ToListAsync();
        return list.Select(g => MapToGradeDto(g, g.Student, g.Teacher!, g.ContractSession)).ToList();
    }

    private static string StatusText(HomeworkStatus s)
    {
        return s switch
        {
            HomeworkStatus.Assigned => "Assigned",
            HomeworkStatus.Submitted => "Submitted",
            HomeworkStatus.Graded => "Graded",
            HomeworkStatus.Overdue => "Overdue",
            _ => s.ToString()
        };
    }

    private static HomeworkDto MapToHomeworkDto(Homework h, Student student, TeacherProfile teacher, ContractSession? contract)
    {
        return new HomeworkDto
        {
            Id = h.Id,
            StudentId = h.StudentId,
            StudentName = $"{student.FirstName} {student.LastName}",
            TeacherId = h.TeacherId,
            TeacherName = $"{teacher.User.FirstName} {teacher.User.LastName}",
            ContractSessionId = h.ContractSessionId,
            ContractIdDisplay = contract?.ContractId,
            Title = h.Title,
            Description = h.Description,
            DueDate = h.DueDate,
            Status = h.Status,
            StatusText = StatusText(h.Status),
            SubmittedAt = h.SubmittedAt,
            GradedAt = h.GradedAt,
            TeacherFeedback = h.TeacherFeedback,
            CreatedAt = h.CreatedAt
        };
    }

    private static StudentGradeDto MapToGradeDto(StudentGrade g, Student student, TeacherProfile teacher, ContractSession? contract)
    {
        return new StudentGradeDto
        {
            Id = g.Id,
            StudentId = g.StudentId,
            StudentName = $"{student.FirstName} {student.LastName}",
            TeacherId = g.TeacherId,
            TeacherName = $"{teacher.User.FirstName} {teacher.User.LastName}",
            ContractSessionId = g.ContractSessionId,
            ContractIdDisplay = contract?.ContractId,
            Title = g.Title,
            GradeValue = g.GradeValue,
            MaxValue = g.MaxValue,
            GradeDate = g.GradeDate,
            Notes = g.Notes,
            CreatedAt = g.CreatedAt
        };
    }

    public static HomeworkItemDto ToHomeworkItemDto(Homework h, string teacherName)
    {
        return new HomeworkItemDto
        {
            Id = h.Id,
            Title = h.Title,
            Description = h.Description,
            DueDate = h.DueDate,
            Status = (int)h.Status,
            StatusText = StatusText(h.Status),
            TeacherFeedback = h.TeacherFeedback,
            TeacherName = teacherName
        };
    }

    public static GradeItemDto ToGradeItemDto(StudentGrade g, string teacherName)
    {
        return new GradeItemDto
        {
            Id = g.Id,
            Title = g.Title,
            GradeValue = g.GradeValue,
            MaxValue = g.MaxValue,
            GradeDate = g.GradeDate,
            Notes = g.Notes,
            TeacherName = teacherName
        };
    }
}
