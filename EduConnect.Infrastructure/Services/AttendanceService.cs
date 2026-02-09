using EduConnect.Application.Common.Exceptions;
using EduConnect.Application.DTOs.GroupClass;
using EduConnect.Application.Features.Attendance.Interfaces;
using EduConnect.Application.Features.Notifications.Interfaces;
using EduConnect.Domain.Entities;
using EduConnect.Infrastructure.Data;
using EduConnect.Infrastructure.Repositories;
using EduConnect.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public AttendanceService(ApplicationDbContext context, IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<int> CheckInAsync(int teacherId, int contractId)
    {
        var contract = await _context.ContractSessions
            .FirstOrDefaultAsync(c => c.Id == contractId && c.TeacherId == teacherId && c.Status == ContractStatus.Active);
        if (contract == null)
            throw new NotFoundException("Contract", contractId);
        if (contract.RemainingHours <= 0)
            throw new EduConnect.Application.Common.Exceptions.BusinessException("No remaining hours on this contract.", "NO_HOURS");

        var alreadyInProgress = await _context.AttendanceLogs
            .AnyAsync(a => a.ContractId == contractId && a.CheckOutTime == null);
        if (alreadyInProgress)
            throw new EduConnect.Application.Common.Exceptions.BusinessException("A session is already in progress for this contract.", "SESSION_IN_PROGRESS");

        var log = new AttendanceLog
        {
            ContractId = contractId,
            SessionId = 0,
            CheckInTime = DateTime.UtcNow,
            Status = SessionStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };
        _context.AttendanceLogs.Add(log);
        await _unitOfWork.SaveChangesAsync();
        log.SessionId = log.Id;
        await _unitOfWork.SaveChangesAsync();
        return log.Id;
    }

    public async Task<bool> CheckOutAsync(int teacherId, int attendanceLogId, string lessonNotes)
    {
        if (string.IsNullOrWhiteSpace(lessonNotes))
            throw new EduConnect.Application.Common.Exceptions.BusinessException("Lesson notes are required to check out.", "NOTES_REQUIRED");

        var log = await _context.AttendanceLogs
            .Include(a => a.ContractSession).ThenInclude(c => c!.Student)
            .FirstOrDefaultAsync(a => a.Id == attendanceLogId && a.ContractSession!.TeacherId == teacherId);
        if (log == null)
            throw new NotFoundException("Attendance session", attendanceLogId);
        if (log.CheckOutTime != null)
            throw new EduConnect.Application.Common.Exceptions.BusinessException("Session already checked out.", "ALREADY_CHECKED_OUT");

        var now = DateTime.UtcNow;
        log.CheckOutTime = now;
        log.HoursUsed = (decimal)(now - log.CheckInTime).TotalHours;
        log.LessonNotes = lessonNotes.Trim();
        log.Status = SessionStatus.Completed;

        if (log.ContractSession != null)
        {
            var hoursToDeduct = (int)Math.Ceiling(log.HoursUsed);
            log.ContractSession.RemainingHours = Math.Max(0, log.ContractSession.RemainingHours - hoursToDeduct);
        }

        await _unitOfWork.SaveChangesAsync();

        if (log.ContractSession?.Student != null)
        {
            var studentName = $"{log.ContractSession.Student.FirstName} {log.ContractSession.Student.LastName}";
            await _notificationService.CreateForUserAsync(log.ContractSession.Student.ParentId, "Session completed – notes added", $"Session for {studentName} has been completed with lesson notes.", NotificationType.SessionCompleted, "Attendance", log.Id);
        }
        return true;
    }

    public async Task<int> CheckInGroupAsync(int teacherId, int groupClassId)
    {
        var group = await _context.GroupClasses
            .Include(g => g.Enrollments)
            .FirstOrDefaultAsync(g => g.Id == groupClassId && g.TeacherId == teacherId && g.IsActive);
        if (group == null)
            throw new NotFoundException("Group class", groupClassId);
        if (group.Enrollments.Count == 0)
            throw new BusinessException("Group class has no enrolled students.", "NO_ENROLLMENTS");
        var inProgress = await _context.GroupSessions
            .AnyAsync(s => s.GroupClassId == groupClassId && s.CheckOutTime == null);
        if (inProgress)
            throw new BusinessException("A group session is already in progress for this class.", "SESSION_IN_PROGRESS");
        var session = new GroupSession
        {
            GroupClassId = groupClassId,
            CheckInTime = DateTime.UtcNow,
            Status = SessionStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };
        _context.GroupSessions.Add(session);
        await _unitOfWork.SaveChangesAsync();
        return session.Id;
    }

    public async Task<bool> CheckOutGroupAsync(int teacherId, int groupSessionId, string lessonNotes)
    {
        if (string.IsNullOrWhiteSpace(lessonNotes))
            throw new BusinessException("Lesson notes are required to check out.", "NOTES_REQUIRED");
        var session = await _context.GroupSessions
            .Include(s => s.GroupClass).ThenInclude(g => g!.Enrollments).ThenInclude(e => e.ContractSession).ThenInclude(c => c!.Student)
            .FirstOrDefaultAsync(s => s.Id == groupSessionId && s.GroupClass!.TeacherId == teacherId);
        if (session == null)
            throw new NotFoundException("Group session", groupSessionId);
        if (session.CheckOutTime != null)
            throw new BusinessException("Session already checked out.", "ALREADY_CHECKED_OUT");
        var now = DateTime.UtcNow;
        session.CheckOutTime = now;
        session.TotalDurationHours = (decimal)(now - session.CheckInTime).TotalHours;
        session.LessonNotes = lessonNotes.Trim();
        session.Status = SessionStatus.Completed;
        var enrollments = session.GroupClass?.Enrollments?.ToList() ?? new List<GroupClassEnrollment>();
        var count = enrollments.Count;
        if (count == 0) { await _unitOfWork.SaveChangesAsync(); return true; }
        var hoursPerStudent = Math.Max(0.25m, session.TotalDurationHours / count);
        var hoursToDeductPerStudent = (int)Math.Ceiling(hoursPerStudent);
        foreach (var e in enrollments)
        {
            var contract = e.ContractSession;
            if (contract != null)
            {
                var deduct = Math.Min(hoursToDeductPerStudent, contract.RemainingHours);
                contract.RemainingHours = Math.Max(0, contract.RemainingHours - deduct);
            }
            _context.GroupSessionAttendances.Add(new GroupSessionAttendance
            {
                GroupSessionId = session.Id,
                StudentId = e.StudentId,
                ContractId = e.ContractId,
                HoursUsed = hoursPerStudent
            });
            if (e.ContractSession?.Student != null)
            {
                var studentName = $"{e.ContractSession.Student.FirstName} {e.ContractSession.Student.LastName}";
                await _notificationService.CreateForUserAsync(e.ContractSession.Student.ParentId, "Session completed – notes added", $"Group session for {session.GroupClass?.Name} ({studentName}) has been completed with lesson notes.", NotificationType.SessionCompleted, "GroupSession", session.Id);
            }
        }
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<GroupSessionDto>> GetGroupSessionsByTeacherAsync(int teacherId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.GroupSessions
            .AsNoTracking()
            .Include(s => s.GroupClass)
            .Where(s => s.GroupClass!.TeacherId == teacherId);
        if (from.HasValue)
            query = query.Where(s => s.CheckInTime >= from.Value);
        if (to.HasValue)
            query = query.Where(s => s.CheckInTime < to.Value);
        var list = await query.OrderByDescending(s => s.CheckInTime).Take(100).ToListAsync();
        var result = new List<GroupSessionDto>();
        foreach (var s in list)
        {
            var attendeeCount = await _context.GroupSessionAttendances.CountAsync(a => a.GroupSessionId == s.Id);
            result.Add(new GroupSessionDto
            {
                Id = s.Id,
                GroupClassId = s.GroupClassId,
                GroupClassName = s.GroupClass?.Name ?? "",
                CheckInTime = s.CheckInTime,
                CheckOutTime = s.CheckOutTime,
                TotalDurationHours = s.TotalDurationHours,
                LessonNotes = s.LessonNotes,
                ZoomJoinUrl = s.ZoomJoinUrl ?? s.GroupClass?.ZoomJoinUrl,
                Status = (int)s.Status,
                AttendeeCount = attendeeCount
            });
        }
        return result;
    }

    public async Task<AttendanceSessionDto> GetSessionByIdAsync(int sessionId)
    {
        var log = await _context.AttendanceLogs
            .Include(a => a.ContractSession)
            .FirstOrDefaultAsync(a => a.Id == sessionId || a.SessionId == sessionId);
        if (log == null)
            throw new NotFoundException("Session", sessionId);
        return MapToDto(log);
    }

    public async Task<List<AttendanceSessionDto>> GetSessionsByContractAsync(int contractId)
    {
        var logs = await _context.AttendanceLogs
            .Where(a => a.ContractId == contractId)
            .OrderByDescending(a => a.CheckInTime)
            .ToListAsync();
        return logs.Select(MapToDto).ToList();
    }

    private static AttendanceSessionDto MapToDto(AttendanceLog a)
    {
        return new AttendanceSessionDto
        {
            Id = a.Id,
            SessionId = a.SessionId,
            ContractId = a.ContractId,
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            HoursUsed = a.HoursUsed,
            LessonNotes = a.LessonNotes,
            ProgressReport = a.ProgressReport,
            Status = (int)a.Status
        };
    }
}
