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
            .Where(c => c.TeacherId == teacherId && c.Status == ContractStatus.Active)
            .ToListAsync();

        var todayLogs = await _context.AttendanceLogs
            .Include(a => a.ContractSession!).ThenInclude(c => c.Student)
            .Where(a => a.ContractSession!.TeacherId == teacherId && a.CheckInTime >= todayStartUtc && a.CheckInTime < todayEndUtc)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();

        var totalRemaining = contracts.Sum(c => c.RemainingHours);

        var todaySessions = todayLogs.Select(a => MapToSessionItem(a)).ToList();

        var inProgressLog = todayLogs.FirstOrDefault(a => a.CheckOutTime == null);
        var upcoming = contracts
            .Where(c => c.RemainingHours > 0 && !todayLogs.Any(l => l.ContractId == c.Id && l.CheckOutTime == null))
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
            TotalRemainingHours = totalRemaining,
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
            VerificationStatus = t.VerificationStatus.ToString()
        };
    }

    public async Task<List<TeacherAssignedStudentDto>> GetAssignedStudentsAsync(int teacherId)
    {
        var contracts = await _context.ContractSessions
            .Include(c => c.Student)
            .Include(c => c.Teacher)
            .Where(c => c.TeacherId == teacherId && c.Status == ContractStatus.Active)
            .ToListAsync();
        return contracts.Select(c => new TeacherAssignedStudentDto
        {
            StudentId = c.StudentId,
            StudentName = $"{c.Student!.FirstName} {c.Student.LastName}",
            GradeLevel = c.Student.GradeLevel.ToString(),
            Subjects = c.Teacher?.Specializations ?? "",
            ContractStatus = c.Status.ToString(),
            ContractIdDisplay = c.ContractId,
            RemainingHours = c.RemainingHours
        }).ToList();
    }

    public async Task<List<TeacherSessionItemDto>> GetTodaySessionsAsync(int teacherId)
    {
        var (todayStartUtc, todayEndUtc) = MyanmarTimeHelper.GetTodayUtcRange();
        var logs = await _context.AttendanceLogs
            .Include(a => a.ContractSession!).ThenInclude(c => c.Student)
            .Where(a => a.ContractSession!.TeacherId == teacherId && a.CheckInTime >= todayStartUtc && a.CheckInTime < todayEndUtc)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();
        return logs.Select(MapToSessionItem).ToList();
    }

    public async Task<List<TeacherSessionItemDto>> GetUpcomingSessionsAsync(int teacherId)
    {
        var contracts = await _context.ContractSessions
            .Include(c => c.Student)
            .Where(c => c.TeacherId == teacherId && c.Status == ContractStatus.Active && c.RemainingHours > 0)
            .ToListAsync();
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
                CanCheckOut = false
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

    private static TeacherSessionItemDto MapToSessionItem(AttendanceLog a)
    {
        var canCheckOut = a.CheckOutTime == null && !string.IsNullOrEmpty(a.LessonNotes) == false;
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
            CanCheckOut = a.CheckOutTime == null
        };
    }
}
