using EduConnect.Application.Common.Exceptions;
using EduConnect.Application.Features.Attendance.Interfaces;
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

    public AttendanceService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
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
            .Include(a => a.ContractSession)
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
        return true;
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
