using EduConnect.Application.DTOs.Teacher;
using EduConnect.Application.Features.Teachers.Interfaces;
using EduConnect.Domain.Entities;
using EduConnect.Infrastructure.Data;
using EduConnect.Infrastructure.Repositories;
using EduConnect.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastructure.Services;

public class TeacherService : ITeacherService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public TeacherService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<int?> GetTeacherIdByUserIdAsync(string userId)
    {
        var profile = await _context.TeacherProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == userId);
        return profile?.Id;
    }

    public async Task<TeacherDashboardDto> GetDashboardAsync(int teacherId)
    {
        var (todayStartUtc, todayEndUtc) = MyanmarTimeHelper.GetTodayUtcRange();
        var contracts = await _context.ContractSessions
            .Include(c => c.Student)
            .Include(c => c.Teacher).ThenInclude(t => t!.User)
            .Include(c => c.Subscription)
            .Where(c => c.TeacherId == teacherId && c.Status == ContractStatus.Active)
            .ToListAsync();

        var todayLogs = await _context.AttendanceLogs
            .Include(a => a.ContractSession!).ThenInclude(c => c.Student)
            .Where(a => a.ContractSession!.TeacherId == teacherId && a.CheckInTime >= todayStartUtc && a.CheckInTime < todayEndUtc)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();

        var todaySessions = todayLogs.Select(a => MapToSessionItem(a)).ToList();

        var inProgressLog = todayLogs.FirstOrDefault(a => a.CheckOutTime == null);
        var upcoming = contracts
            .Where(c => c.HasActiveAccess() && !todayLogs.Any(l => l.ContractId == c.Id && l.CheckOutTime == null))
            .Take(10)
            .Select(c => new TeacherSessionItemDto
            {
                Id = 0,
                ContractId = c.Id,
                ContractIdDisplay = c.ContractId,
                StudentName = $"{c.Student.FirstName} {c.Student.LastName}",
                Status = "Available",
                CanCheckIn = true,
                CanCheckOut = false
            }).ToList();

        var alerts = new List<TeacherAlertDto>();
        if (inProgressLog != null)
            alerts.Add(new TeacherAlertDto { Type = "InProgress", Message = "You have a session in progress. Complete check-out with lesson notes." });
        foreach (var log in todayLogs.Where(a => a.CheckOutTime != null && string.IsNullOrEmpty(a.LessonNotes)))
            alerts.Add(new TeacherAlertDto { Type = "MissingNotes", Message = "Lesson notes required for completed session." });

        return new TeacherDashboardDto
        {
            TodaySessions = todaySessions,
            UpcomingSessions = upcoming,
            TotalRemainingHours = 0,
            Alerts = alerts
        };
    }

    public async Task<TeacherProfileDto> GetProfileAsync(int teacherId)
    {
        var t = await _context.TeacherProfiles
            .Include(x => x.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == teacherId);
        if (t == null) throw new EduConnect.Application.Common.Exceptions.NotFoundException("Teacher", teacherId);
        return new TeacherProfileDto
        {
            FirstName = t.User.FirstName,
            LastName = t.User.LastName,
            Email = t.User.Email ?? string.Empty,
            PhoneNumber = t.User.PhoneNumber ?? string.Empty,
            EducationLevel = t.EducationLevel,
            Bio = t.Bio,
            Specializations = t.Specializations,
            ZoomJoinUrl = t.ZoomJoinUrl,
            VerificationStatus = t.VerificationStatus.ToString()
        };
    }

    public async Task UpdateZoomJoinUrlAsync(int teacherId, string? zoomJoinUrl)
    {
        var t = await _context.TeacherProfiles.FirstOrDefaultAsync(x => x.Id == teacherId);
        if (t == null) throw new EduConnect.Application.Common.Exceptions.NotFoundException("Teacher", teacherId);
        t.ZoomJoinUrl = string.IsNullOrWhiteSpace(zoomJoinUrl) ? null : zoomJoinUrl.Trim();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<TeacherAssignedStudentDto>> GetAssignedStudentsAsync(int teacherId)
    {
        var contracts = await _context.ContractSessions
            .Include(c => c.Student)
            .Include(c => c.Teacher)
            .Include(c => c.Subscription)
            .Where(c => c.TeacherId == teacherId && c.Status == ContractStatus.Active)
            .ToListAsync();
        return contracts.Select(c => new TeacherAssignedStudentDto
        {
            StudentId = c.StudentId,
            StudentName = $"{c.Student!.FirstName} {c.Student.LastName}",
            GradeLevel = c.Student.GradeLevel.ToString(),
            Subjects = c.Teacher?.Specializations ?? "",
            ContractStatus = c.Status.ToString(),
            ContractId = c.Id,
            ContractIdDisplay = c.ContractId,
            SubscriptionPeriodEnd = c.Subscription?.SubscriptionPeriodEnd ?? c.SubscriptionPeriodEnd
        }).ToList();
    }

    public async Task<List<TeacherSessionItemDto>> GetTodaySessionsAsync(int teacherId)
    {
        var (todayStartUtc, todayEndUtc) = MyanmarTimeHelper.GetTodayUtcRange();
        var logs = await _context.AttendanceLogs
            .Include(a => a.ContractSession!).ThenInclude(c => c.Student)
            .Include(a => a.ContractSession!).ThenInclude(c => c.Teacher)
            .Where(a => a.ContractSession!.TeacherId == teacherId && a.CheckInTime >= todayStartUtc && a.CheckInTime < todayEndUtc)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();
        return logs.Select(MapToSessionItem).ToList();
    }

    public async Task<List<TeacherSessionItemDto>> GetUpcomingSessionsAsync(int teacherId)
    {
        var contracts = await _context.ContractSessions
            .Include(c => c.Student)
            .Include(c => c.Teacher)
            .Include(c => c.Subscription)
            .Where(c => c.TeacherId == teacherId && c.Status == ContractStatus.Active)
            .ToListAsync();
        contracts = contracts.Where(c => c.HasActiveAccess()).ToList();
        var (todayStartUtc, todayEndUtc) = MyanmarTimeHelper.GetTodayUtcRange();
        var todayLogs = await _context.AttendanceLogs
            .Where(a => a.CheckInTime >= todayStartUtc && a.CheckInTime < todayEndUtc && a.CheckOutTime == null)
            .Join(_context.ContractSessions.Where(c => c.TeacherId == teacherId),
                a => a.ContractId, c => c.Id, (a, c) => a.ContractId)
            .ToListAsync();
        return contracts
            .Where(c => !todayLogs.Contains(c.Id))
            .Take(20)
            .Select(c => new TeacherSessionItemDto
            {
                Id = 0,
                ContractId = c.Id,
                ContractIdDisplay = c.ContractId,
                StudentName = $"{c.Student!.FirstName} {c.Student.LastName}",
                Status = "Available",
                CanCheckIn = true,
                CanCheckOut = false,
                ZoomJoinUrl = c.Teacher?.ZoomJoinUrl
            }).ToList();
    }

    public async Task UpdateAvailabilityAsync(int teacherId, List<TeacherAvailabilityDto> availabilities)
    {
        var existing = await _context.TeacherAvailabilities.Where(a => a.TeacherId == teacherId).ToListAsync();
        _context.TeacherAvailabilities.RemoveRange(existing);
        foreach (var dto in availabilities.Where(a => a.IsAvailable))
        {
            _context.TeacherAvailabilities.Add(new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAvailabilityFromRequestAsync(int teacherId, List<TeacherAvailabilityRequestDto> availabilities)
    {
        var existing = await _context.TeacherAvailabilities.Where(a => a.TeacherId == teacherId).ToListAsync();
        _context.TeacherAvailabilities.RemoveRange(existing);
        foreach (var dto in availabilities.Where(a => a.IsAvailable))
        {
            if (!TimeSpan.TryParse(dto.StartTime, out var start) || !TimeSpan.TryParse(dto.EndTime, out var end))
                continue;
            _context.TeacherAvailabilities.Add(new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = (DayOfWeek)dto.DayOfWeek,
                StartTime = start,
                EndTime = end,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<TeacherAvailabilityDto>> GetAvailabilityAsync(int teacherId)
    {
        var list = await _context.TeacherAvailabilities
            .AsNoTracking()
            .Where(a => a.TeacherId == teacherId)
            .ToListAsync();
        return list.Select(a => new TeacherAvailabilityDto
        {
            Id = a.Id,
            DayOfWeek = a.DayOfWeek,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            IsAvailable = a.IsAvailable
        }).ToList();
    }

    public async Task<List<WeekSessionDto>> GetSessionsForWeekAsync(int teacherId, DateTime weekStartMonday)
    {
        var (startUtc, endUtc) = MyanmarTimeHelper.GetUtcRangeForWeek(weekStartMonday);
        var logs = await _context.AttendanceLogs
            .Include(a => a.ContractSession!).ThenInclude(c => c.Student)
            .Include(a => a.ContractSession!).ThenInclude(c => c.Teacher!).ThenInclude(t => t.User)
            .Where(a => a.ContractSession!.TeacherId == teacherId && a.CheckInTime >= startUtc && a.CheckInTime < endUtc)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();

        var contracts = await _context.ContractSessions
            .Include(c => c.Student)
            .Where(c => c.TeacherId == teacherId && c.Status == ContractStatus.Active
                && !string.IsNullOrWhiteSpace(c.DaysOfWeek) && c.StartTime.HasValue)
            .ToListAsync();

        var groupSessions = await _context.GroupSessions
            .Include(s => s.GroupClass)
            .Where(s => s.GroupClass!.TeacherId == teacherId && s.CheckInTime >= startUtc && s.CheckInTime < endUtc)
            .OrderBy(s => s.CheckInTime)
            .ToListAsync();

        var groupClasses = await _context.GroupClasses
            .AsNoTracking()
            .Where(g => g.TeacherId == teacherId && g.IsActive
                && !string.IsNullOrWhiteSpace(g.DaysOfWeek) && g.StartTime.HasValue)
            .ToListAsync();

        var weekStart = weekStartMonday.Date;
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
        for (var dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var date = weekStart.AddDays(dayOffset);
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
                    StudentName = $"{c.Student.FirstName} {c.Student.LastName}",
                    TeacherName = "",
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
        var result = logs.Select(a => MapToWeekSession(a))
            .Concat(groupSessionDtos)
            .Concat(scheduled)
            .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
            .ToList();
        return result;
    }

    public async Task<List<WeekSessionDto>> GetSessionsForMonthAsync(int teacherId, int year, int month)
    {
        var (startUtc, endUtc) = MyanmarTimeHelper.GetUtcRangeForMonth(year, month);
        var logs = await _context.AttendanceLogs
            .Include(a => a.ContractSession!).ThenInclude(c => c.Student)
            .Include(a => a.ContractSession!).ThenInclude(c => c.Teacher!).ThenInclude(t => t.User)
            .Where(a => a.ContractSession!.TeacherId == teacherId && a.CheckInTime >= startUtc && a.CheckInTime < endUtc)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();

        var contracts = await _context.ContractSessions
            .Include(c => c.Student)
            .Where(c => c.TeacherId == teacherId && c.Status == ContractStatus.Active
                && !string.IsNullOrWhiteSpace(c.DaysOfWeek) && c.StartTime.HasValue)
            .ToListAsync();

        var groupSessions = await _context.GroupSessions
            .Include(s => s.GroupClass)
            .Where(s => s.GroupClass!.TeacherId == teacherId && s.CheckInTime >= startUtc && s.CheckInTime < endUtc)
            .OrderBy(s => s.CheckInTime)
            .ToListAsync();

        var groupClasses = await _context.GroupClasses
            .AsNoTracking()
            .Where(g => g.TeacherId == teacherId && g.IsActive
                && !string.IsNullOrWhiteSpace(g.DaysOfWeek) && g.StartTime.HasValue)
            .ToListAsync();

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
                    StudentName = $"{c.Student.FirstName} {c.Student.LastName}",
                    TeacherName = "",
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
            StudentName = a.ContractSession?.Student != null ? $"{a.ContractSession.Student.FirstName} {a.ContractSession.Student.LastName}" : "",
            TeacherName = "",
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

    private static TeacherSessionItemDto MapToSessionItem(AttendanceLog a)
    {
        return new TeacherSessionItemDto
        {
            Id = a.Id,
            ContractId = a.ContractId,
            ContractIdDisplay = a.ContractSession?.ContractId ?? "",
            StudentName = a.ContractSession?.Student != null ? $"{a.ContractSession.Student.FirstName} {a.ContractSession.Student.LastName}" : "",
            Status = a.Status.ToString(),
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            LessonNotes = a.LessonNotes,
            CanCheckIn = false,
            CanCheckOut = a.CheckOutTime == null,
            ZoomJoinUrl = a.ZoomJoinUrl ?? a.ContractSession?.Teacher?.ZoomJoinUrl
        };
    }
}
