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
            .OrderBy(s => s.FullName)
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
                FullName = s.FullName ?? string.Empty,
                GradeLevel = s.GradeLevel.ToString(),
                SubscriptionValidUntil = subscriptionValidUntil,
                AssignedTeacherName = firstTeacher?.User?.FullName,
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
                TeacherName = c.Teacher?.User?.FullName ?? "",
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
                TeacherName = c.Teacher?.User?.FullName ?? "",
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
                ? a.ContractSession.Teacher.User.FullName
                : ""
        }).ToList();

        var homeworks = await _context.Homeworks
            .Include(h => h.Teacher).ThenInclude(t => t!.User)
            .Where(h => h.StudentId == student.Id)
            .OrderByDescending(h => h.DueDate)
            .Take(50)
            .ToListAsync();
        var homeworkItems = homeworks.Select(h => HomeworkService.ToHomeworkItemDto(h, h.Teacher?.User?.FullName ?? "")).ToList();

        var grades = await _context.StudentGrades
            .Include(g => g.Teacher).ThenInclude(t => t!.User)
            .Where(g => g.StudentId == student.Id)
            .OrderByDescending(g => g.GradeDate)
            .Take(50)
            .ToListAsync();
        var gradeItems = grades.Select(g => HomeworkService.ToGradeItemDto(g, g.Teacher?.User?.FullName ?? "")).ToList();

        var periodEnds = activeContracts.Select(c => c.Subscription?.SubscriptionPeriodEnd ?? c.SubscriptionPeriodEnd).Where(d => d.HasValue).Select(d => d!.Value).ToList();
        var subscriptionValidUntil = periodEnds.Any() ? periodEnds.Max() : (DateTime?)null;
        return new StudentLearningOverviewDto
        {
            StudentId = student.Id,
            StudentName = student.FullName ?? string.Empty,
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

    public async Task<List<WeekSessionDto>> GetSessionsForStudentMonthAsync(string parentUserId, int studentId, int year, int month)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId && s.ParentId == parentUserId);
        if (student == null)
            return new List<WeekSessionDto>();

        var now = DateTime.UtcNow;
        var contracts = await _context.ContractSessions
            .Include(c => c.Student)
            .Include(c => c.Teacher!).ThenInclude(t => t!.User)
            .Include(c => c.Subscription)
            .Where(c => c.StudentId == studentId && c.Status == ContractStatus.Active
                && !string.IsNullOrWhiteSpace(c.DaysOfWeek) && c.StartTime.HasValue
                && ((c.SubscriptionId == null && c.SubscriptionPeriodEnd != null && c.SubscriptionPeriodEnd.Value.Date >= now.Date)
                    || (c.SubscriptionId != null && c.Subscription!.Status == ContractStatus.Active && c.Subscription.SubscriptionPeriodEnd.Date >= now.Date)))
            .ToListAsync();
        var contractIds = contracts.Select(c => c.Id).ToList();

        var enrollments = await _context.GroupClassEnrollments
            .Include(e => e.GroupClass)
            .Include(e => e.Subscription)
            .Include(e => e.ContractSession!).ThenInclude(c => c!.Subscription)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
        var enrollmentsWithAccess = enrollments
            .Where(e => e.GroupClass != null && e.GroupClass.IsActive && HasGroupEnrollmentAccess(e, now))
            .ToList();
        var groupClassIds = enrollmentsWithAccess.Select(e => e.GroupClassId).Distinct().ToList();

        if (contractIds.Count == 0 && groupClassIds.Count == 0)
            return new List<WeekSessionDto>();

        var (startUtc, endUtc) = MyanmarTimeHelper.GetUtcRangeForMonth(year, month);
        var logs = contractIds.Count > 0
            ? await _context.AttendanceLogs
                .Include(a => a.ContractSession!).ThenInclude(c => c.Student)
                .Include(a => a.ContractSession!).ThenInclude(c => c.Teacher!).ThenInclude(t => t.User)
                .Where(a => contractIds.Contains(a.ContractId) && a.CheckInTime >= startUtc && a.CheckInTime < endUtc)
                .OrderBy(a => a.CheckInTime)
                .ToListAsync()
            : new List<AttendanceLog>();

        var groupSessions = groupClassIds.Count > 0
            ? await _context.GroupSessions
                .Include(s => s.GroupClass)
                .Where(s => groupClassIds.Contains(s.GroupClassId) && s.CheckInTime >= startUtc && s.CheckInTime < endUtc)
                .OrderBy(s => s.CheckInTime)
                .ToListAsync()
            : new List<GroupSession>();

        var groupClasses = groupClassIds.Count > 0
            ? await _context.GroupClasses
                .AsNoTracking()
                .Where(g => groupClassIds.Contains(g.Id) && g.IsActive && !string.IsNullOrWhiteSpace(g.DaysOfWeek) && g.StartTime.HasValue)
                .ToListAsync()
            : new List<GroupClass>();

        var logDatesByContract = new HashSet<(int ContractId, DateTime Date)>();
        foreach (var a in logs)
        {
            var dateMyanmar = MyanmarTimeHelper.UtcToMyanmarDate(a.CheckInTime);
            logDatesByContract.Add((a.ContractId, dateMyanmar.Date));
        }
        var groupSessionDatesByClass = new HashSet<(int GroupClassId, DateTime Date)>();
        foreach (var gs in groupSessions)
        {
            var dateMyanmar = MyanmarTimeHelper.UtcToMyanmarDate(gs.CheckInTime);
            groupSessionDatesByClass.Add((gs.GroupClassId, dateMyanmar.Date));
        }

        var scheduled = new List<WeekSessionDto>();
        var daysInMonth = DateTime.DaysInMonth(year, month);
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
            var isoDay = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;

            foreach (var c in contracts)
            {
                var contractDays = ParseDaysOfWeek(c.DaysOfWeek!);
                if (!contractDays.Contains(isoDay)) continue;

                var startMyanmar = MyanmarTimeHelper.UtcToMyanmarDate(c.StartDate);
                if (date < startMyanmar.Date) continue;
                if (c.EndDate.HasValue)
                {
                    var endMyanmar = MyanmarTimeHelper.UtcToMyanmarDate(c.EndDate.Value);
                    if (date > endMyanmar.Date) continue;
                }

                if (logDatesByContract.Contains((c.Id, date))) continue;

                var startTime = c.StartTime!.Value.ToString("HH:mm");
                var endTime = c.EndTime.HasValue ? c.EndTime.Value.ToString("HH:mm") : null;
                scheduled.Add(new WeekSessionDto
                {
                    AttendanceLogId = 0,
                    ContractId = c.Id,
                    ContractIdDisplay = c.ContractId,
                    Date = date,
                    DateYmd = date.ToString("yyyy-MM-dd"),
                    StartTime = startTime,
                    EndTime = endTime,
                    StudentName = c.Student?.FullName ?? "",
                    TeacherName = c.Teacher?.User?.FullName ?? "",
                    Status = "Scheduled",
                    HoursUsed = 0
                });
            }

            foreach (var gc in groupClasses)
            {
                var groupDays = ParseDaysOfWeek(gc.DaysOfWeek!);
                if (!groupDays.Contains(isoDay)) continue;
                if (groupSessionDatesByClass.Contains((gc.Id, date))) continue;

                scheduled.Add(new WeekSessionDto
                {
                    AttendanceLogId = 0,
                    ContractId = 0,
                    ContractIdDisplay = "",
                    Date = date,
                    DateYmd = date.ToString("yyyy-MM-dd"),
                    StartTime = gc.StartTime!.Value.ToString("HH:mm"),
                    EndTime = gc.EndTime.HasValue ? gc.EndTime.Value.ToString("HH:mm") : null,
                    StudentName = "",
                    TeacherName = "",
                    Status = "Scheduled",
                    HoursUsed = 0,
                    GroupClassId = gc.Id,
                    GroupClassName = gc.Name
                });
            }
        }

        var groupSessionDtos = groupSessions.Select(gs => MapGroupSessionToWeek(gs));
        return logs.Select(a => MapToWeekSession(a))
            .Concat(groupSessionDtos)
            .Concat(scheduled)
            .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
            .ToList();
    }

    private static bool HasGroupEnrollmentAccess(GroupClassEnrollment e, DateTime now)
    {
        if (e.SubscriptionId != null && e.Subscription != null)
            return e.Subscription.Status == ContractStatus.Active && e.Subscription.SubscriptionPeriodEnd.Date >= now.Date;
        if (e.ContractId != null && e.ContractSession != null)
        {
            var c = e.ContractSession;
            if (c.Status != ContractStatus.Active) return false;
            if (c.SubscriptionId != null && c.Subscription != null)
                return c.Subscription.Status == ContractStatus.Active && c.Subscription.SubscriptionPeriodEnd.Date >= now.Date;
            return c.SubscriptionPeriodEnd != null && c.SubscriptionPeriodEnd.Value.Date >= now.Date;
        }
        return false;
    }

    private static HashSet<int> ParseDaysOfWeek(string daysOfWeek)
    {
        var set = new HashSet<int>();
        foreach (var part in (daysOfWeek ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (int.TryParse(part, out var d) && d >= 1 && d <= 7)
                set.Add(d);
        return set;
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
            DateYmd = dateMyanmar.ToString("yyyy-MM-dd"),
            StartTime = MyanmarTimeHelper.FormatTimeUtcToMyanmar(a.CheckInTime),
            EndTime = a.CheckOutTime.HasValue ? MyanmarTimeHelper.FormatTimeUtcToMyanmar(a.CheckOutTime.Value) : null,
            StudentName = a.ContractSession?.Student?.FullName ?? "",
            TeacherName = a.ContractSession?.Teacher?.User?.FullName ?? "",
            Status = a.Status.ToString(),
            HoursUsed = a.HoursUsed
        };
    }

    private static WeekSessionDto MapGroupSessionToWeek(GroupSession gs)
    {
        var dateMyanmar = MyanmarTimeHelper.UtcToMyanmarDate(gs.CheckInTime);
        return new WeekSessionDto
        {
            AttendanceLogId = 0,
            ContractId = 0,
            ContractIdDisplay = "",
            Date = dateMyanmar,
            DateYmd = dateMyanmar.ToString("yyyy-MM-dd"),
            StartTime = MyanmarTimeHelper.FormatTimeUtcToMyanmar(gs.CheckInTime),
            EndTime = gs.CheckOutTime.HasValue ? MyanmarTimeHelper.FormatTimeUtcToMyanmar(gs.CheckOutTime.Value) : null,
            StudentName = "",
            TeacherName = "",
            Status = gs.CheckOutTime.HasValue ? "Completed" : "InProgress",
            HoursUsed = gs.TotalDurationHours,
            GroupClassId = gs.GroupClassId,
            GroupClassName = gs.GroupClass?.Name ?? "",
            GroupSessionId = gs.Id
        };
    }
}
