using EduConnect.Application.Common.Exceptions;
using EduConnect.Application.DTOs.Parent;
using EduConnect.Application.DTOs.Teacher;
using EduConnect.Application.Features.Parents.Interfaces;
using EduConnect.Domain.Entities;
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
            .Include(s => s.ContractSessions).ThenInclude(c => c.Teacher).ThenInclude(t => t!.User)
            .Include(s => s.ContractSessions).ThenInclude(c => c.Subscription)
            .Where(s => s.ParentId == parentUserId && s.IsActive)
            .OrderBy(s => s.FirstName)
            .ToListAsync();

        return students.Select(s =>
        {
            var activeContracts = s.ContractSessions.Where(c => c.Status == ContractStatus.Active).ToList();
            var periodEnds = activeContracts.Select(c => c.Subscription?.SubscriptionPeriodEnd ?? c.SubscriptionPeriodEnd).Where(d => d.HasValue).Select(d => d!.Value).ToList();
            var subscriptionValidUntil = periodEnds.Any() ? periodEnds.Max() : (DateTime?)null;
            var firstTeacher = activeContracts.FirstOrDefault()?.Teacher;
            return new ParentStudentDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                GradeLevel = s.GradeLevel.ToString(),
                SubscriptionValidUntil = subscriptionValidUntil,
                AssignedTeacherName = firstTeacher != null ? $"{firstTeacher.User.FirstName} {firstTeacher.User.LastName}" : null,
                ActiveContractsCount = activeContracts.Count
            };
        }).ToList();
    }

    public async Task<StudentLearningOverviewDto?> GetStudentLearningOverviewAsync(string parentUserId, int studentId)
    {
        var student = await _context.Students
            .Include(s => s.ContractSessions).ThenInclude(c => c.Teacher).ThenInclude(t => t!.User)
            .Include(s => s.ContractSessions).ThenInclude(c => c.Subscription)
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
                SubscriptionPeriodEnd = c.Subscription?.SubscriptionPeriodEnd ?? c.SubscriptionPeriodEnd
            })
            .ToList();

        var subjects = activeContracts
            .Select(c => c.Teacher?.Specializations)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct();
        var subjectsStr = string.Join(", ", subjects);

        var upcoming = activeContracts
            .Where(c => c.HasActiveAccess())
            .Select(c => new UpcomingSessionDto
            {
                ContractIdDisplay = c.ContractId,
                TeacherName = $"{c.Teacher!.User.FirstName} {c.Teacher.User.LastName}",
                SubscriptionPeriodEnd = c.Subscription?.SubscriptionPeriodEnd ?? c.SubscriptionPeriodEnd
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

        var homeworks = await _context.Homeworks
            .Include(h => h.Teacher).ThenInclude(t => t!.User)
            .Where(h => h.StudentId == student.Id)
            .OrderByDescending(h => h.DueDate)
            .Take(50)
            .ToListAsync();
        var homeworkItems = homeworks.Select(h => HomeworkService.ToHomeworkItemDto(h, $"{h.Teacher.User.FirstName} {h.Teacher.User.LastName}")).ToList();

        var grades = await _context.StudentGrades
            .Include(g => g.Teacher).ThenInclude(t => t!.User)
            .Where(g => g.StudentId == student.Id)
            .OrderByDescending(g => g.GradeDate)
            .Take(50)
            .ToListAsync();
        var gradeItems = grades.Select(g => HomeworkService.ToGradeItemDto(g, $"{g.Teacher.User.FirstName} {g.Teacher.User.LastName}")).ToList();

        var periodEnds = activeContracts.Select(c => c.Subscription?.SubscriptionPeriodEnd ?? c.SubscriptionPeriodEnd).Where(d => d.HasValue).Select(d => d!.Value).ToList();
        var subscriptionValidUntil = periodEnds.Any() ? periodEnds.Max() : (DateTime?)null;
        return new StudentLearningOverviewDto
        {
            StudentId = student.Id,
            StudentName = $"{student.FirstName} {student.LastName}",
            GradeLevel = student.GradeLevel.ToString(),
            AssignedTeachers = assignedTeachers,
            Subjects = subjectsStr,
            SubscriptionValidUntil = subscriptionValidUntil,
            UpcomingSessions = upcoming,
            CompletedSessions = completed,
            Homeworks = homeworkItems,
            Grades = gradeItems
        };
    }

    public async Task<List<WeekSessionDto>> GetSessionsForStudentWeekAsync(string parentUserId, int studentId, DateTime weekStartMonday)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId && s.ParentId == parentUserId);
        if (student == null)
            return new List<WeekSessionDto>();

        var contractIds = await _context.ContractSessions
            .Where(c => c.StudentId == studentId)
            .Select(c => c.Id)
            .ToListAsync();
        if (contractIds.Count == 0)
            return new List<WeekSessionDto>();

        var (startUtc, endUtc) = MyanmarTimeHelper.GetUtcRangeForWeek(weekStartMonday);
        var logs = await _context.AttendanceLogs
            .Include(a => a.ContractSession!).ThenInclude(c => c.Student)
            .Include(a => a.ContractSession!).ThenInclude(c => c.Teacher!).ThenInclude(t => t.User)
            .Where(a => contractIds.Contains(a.ContractId) && a.CheckInTime >= startUtc && a.CheckInTime < endUtc)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();
        return logs.Select(a => MapToWeekSession(a)).ToList();
    }

    private static WeekSessionDto MapToWeekSession(AttendanceLog a)
    {
        var dateMyanmar = MyanmarTimeHelper.UtcToMyanmarDate(a.CheckInTime);
        return new WeekSessionDto
        {
            AttendanceLogId = a.Id,
            ContractId = a.ContractId,
            ContractIdDisplay = a.ContractSession?.ContractId ?? "",
            Date = dateMyanmar,
            StartTime = MyanmarTimeHelper.FormatTimeUtcToMyanmar(a.CheckInTime),
            EndTime = a.CheckOutTime.HasValue ? MyanmarTimeHelper.FormatTimeUtcToMyanmar(a.CheckOutTime.Value) : null,
            StudentName = a.ContractSession?.Student != null ? $"{a.ContractSession.Student.FirstName} {a.ContractSession.Student.LastName}" : "",
            TeacherName = a.ContractSession?.Teacher != null ? $"{a.ContractSession.Teacher.User.FirstName} {a.ContractSession.Teacher.User.LastName}" : "",
            Status = a.Status.ToString(),
            HoursUsed = a.HoursUsed
        };
    }
}
