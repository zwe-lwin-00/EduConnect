using EduConnect.Application.Common.Exceptions;
using EduConnect.Application.DTOs.Parent;
using EduConnect.Application.Features.Parents.Interfaces;
using EduConnect.Infrastructure.Data;
using EduConnect.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastructure.Services;

public class ParentService : IParentService
{
    private readonly ApplicationDbContext _context;

    public ParentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ParentStudentDto>> GetMyStudentsAsync(string parentUserId)
    {
        var students = await _context.Students
            .Include(s => s.ContractSessions)
            .ThenInclude(c => c.Teacher)
            .ThenInclude(t => t!.User)
            .Where(s => s.ParentId == parentUserId && s.IsActive)
            .OrderBy(s => s.FirstName)
            .ToListAsync();

        return students.Select(s =>
        {
            var activeContracts = s.ContractSessions.Where(c => c.Status == ContractStatus.Active).ToList();
            var totalRemaining = activeContracts.Sum(c => c.RemainingHours);
            var firstTeacher = activeContracts.FirstOrDefault()?.Teacher;
            return new ParentStudentDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                GradeLevel = s.GradeLevel.ToString(),
                TotalRemainingHours = totalRemaining,
                AssignedTeacherName = firstTeacher != null ? $"{firstTeacher.User.FirstName} {firstTeacher.User.LastName}" : null,
                ActiveContractsCount = activeContracts.Count
            };
        }).ToList();
    }

    public async Task<StudentLearningOverviewDto?> GetStudentLearningOverviewAsync(string parentUserId, int studentId)
    {
        var student = await _context.Students
            .Include(s => s.ContractSessions)
            .ThenInclude(c => c.Teacher)
            .ThenInclude(t => t!.User)
            .FirstOrDefaultAsync(s => s.Id == studentId && s.ParentId == parentUserId);

        if (student == null)
            return null;

        var activeContracts = student.ContractSessions.Where(c => c.Status == ContractStatus.Active).ToList();
        var allContractIds = student.ContractSessions.Select(c => c.Id).ToList();

        var completedLogs = await _context.AttendanceLogs
            .Include(a => a.ContractSession!)
            .ThenInclude(c => c.Teacher)
            .ThenInclude(t => t!.User)
            .Where(a => allContractIds.Contains(a.ContractId) && a.CheckOutTime != null)
            .OrderByDescending(a => a.CheckInTime)
            .Take(50)
            .ToListAsync();

        var assignedTeachers = activeContracts
            .Select(c => new AssignedTeacherDto
            {
                TeacherId = c.TeacherId,
                TeacherName = $"{c.Teacher!.User.FirstName} {c.Teacher.User.LastName}",
                ContractIdDisplay = c.ContractId,
                RemainingHours = c.RemainingHours
            })
            .ToList();

        var subjects = activeContracts
            .Select(c => c.Teacher?.Specializations)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct();
        var subjectsStr = string.Join(", ", subjects);

        var upcoming = activeContracts
            .Where(c => c.RemainingHours > 0)
            .Select(c => new UpcomingSessionDto
            {
                ContractIdDisplay = c.ContractId,
                TeacherName = $"{c.Teacher!.User.FirstName} {c.Teacher.User.LastName}",
                RemainingHours = c.RemainingHours
            })
            .ToList();

        var completed = completedLogs.Select(a => new CompletedSessionDto
        {
            SessionId = a.Id,
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            HoursUsed = a.HoursUsed,
            LessonNotes = a.LessonNotes,
            ProgressReport = a.ProgressReport,
            TeacherName = a.ContractSession?.Teacher != null
                ? $"{a.ContractSession.Teacher.User.FirstName} {a.ContractSession.Teacher.User.LastName}"
                : ""
        }).ToList();

        return new StudentLearningOverviewDto
        {
            StudentId = student.Id,
            StudentName = $"{student.FirstName} {student.LastName}",
            GradeLevel = student.GradeLevel.ToString(),
            AssignedTeachers = assignedTeachers,
            Subjects = subjectsStr,
            TotalRemainingHours = activeContracts.Sum(c => c.RemainingHours),
            UpcomingSessions = upcoming,
            CompletedSessions = completed
        };
    }
}
