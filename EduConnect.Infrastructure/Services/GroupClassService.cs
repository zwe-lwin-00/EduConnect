using EduConnect.Application.Common.Exceptions;
using EduConnect.Application.DTOs.GroupClass;
using EduConnect.Application.Features.GroupClass.Interfaces;
using EduConnect.Domain.Entities;
using EduConnect.Infrastructure.Data;
using EduConnect.Shared.Enums;
using EduConnect.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduConnect.Infrastructure.Services;

public class GroupClassService : IGroupClassService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GroupClassService> _logger;

    public GroupClassService(ApplicationDbContext context, ILogger<GroupClassService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GroupClassDto> CreateAsync(int teacherId, string name, string? zoomJoinUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("Group class name is required.", "NAME_REQUIRED");
        var teacherExists = await _context.TeacherProfiles.AnyAsync(t => t.Id == teacherId);
        if (!teacherExists)
            throw new NotFoundException("Teacher", teacherId);
        var group = new GroupClass
        {
            TeacherId = teacherId,
            Name = name.Trim(),
            IsActive = true,
            ZoomJoinUrl = string.IsNullOrWhiteSpace(zoomJoinUrl) ? null : zoomJoinUrl.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _context.GroupClasses.Add(group);
        await _context.SaveChangesAsync();
        _logger.InformationLog("Group class created");
        return MapToDto(group, 0);
    }

    public async Task<List<GroupClassDto>> GetByTeacherAsync(int teacherId)
    {
        var list = await _context.GroupClasses
            .AsNoTracking()
            .Include(g => g.Enrollments)
            .Where(g => g.TeacherId == teacherId)
            .OrderBy(g => g.Name)
            .ToListAsync();
        return list.Select(g => MapToDto(g, g.Enrollments.Count)).ToList();
    }

    public async Task<GroupClassDto?> GetByIdAsync(int groupClassId, int teacherId)
    {
        var g = await _context.GroupClasses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == groupClassId && x.TeacherId == teacherId);
        if (g == null) return null;
        var count = await _context.GroupClassEnrollments.CountAsync(e => e.GroupClassId == groupClassId);
        return MapToDto(g, count);
    }

    public async Task<bool> UpdateAsync(int groupClassId, int teacherId, string name, bool isActive, string? zoomJoinUrl = null)
    {
        var g = await _context.GroupClasses.FirstOrDefaultAsync(x => x.Id == groupClassId && x.TeacherId == teacherId);
        if (g == null) return false;
        if (!string.IsNullOrWhiteSpace(name)) g.Name = name.Trim();
        g.IsActive = isActive;
        g.ZoomJoinUrl = string.IsNullOrWhiteSpace(zoomJoinUrl) ? null : zoomJoinUrl.Trim();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnrollStudentAsync(int groupClassId, int teacherId, int studentId, int contractId)
    {
        var group = await _context.GroupClasses.FirstOrDefaultAsync(g => g.Id == groupClassId && g.TeacherId == teacherId);
        if (group == null) throw new NotFoundException("Group class", groupClassId);
        var contract = await _context.ContractSessions
            .FirstOrDefaultAsync(c => c.Id == contractId && c.TeacherId == teacherId && c.StudentId == studentId && c.Status == ContractStatus.Active);
        if (contract == null) throw new NotFoundException("Contract", contractId);
        if (contract.RemainingHours <= 0)
            throw new BusinessException("Contract has no remaining hours.", "NO_HOURS");
        var exists = await _context.GroupClassEnrollments
            .AnyAsync(e => e.GroupClassId == groupClassId && e.StudentId == studentId);
        if (exists) throw new BusinessException("Student is already enrolled in this group class.", "ALREADY_ENROLLED");
        _context.GroupClassEnrollments.Add(new GroupClassEnrollment
        {
            GroupClassId = groupClassId,
            StudentId = studentId,
            ContractId = contractId,
            EnrolledAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnenrollAsync(int enrollmentId, int teacherId)
    {
        var e = await _context.GroupClassEnrollments
            .Include(x => x.GroupClass)
            .FirstOrDefaultAsync(x => x.Id == enrollmentId && x.GroupClass.TeacherId == teacherId);
        if (e == null) return false;
        _context.GroupClassEnrollments.Remove(e);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<GroupClassEnrollmentDto>> GetEnrollmentsAsync(int groupClassId, int teacherId)
    {
        var list = await _context.GroupClassEnrollments
            .AsNoTracking()
            .Include(e => e.GroupClass)
            .Include(e => e.Student)
            .Include(e => e.ContractSession)
            .Where(e => e.GroupClassId == groupClassId && e.GroupClass.TeacherId == teacherId)
            .OrderBy(e => e.Student.FirstName)
            .ToListAsync();
        return list.Select(e => new GroupClassEnrollmentDto
        {
            Id = e.Id,
            GroupClassId = e.GroupClassId,
            StudentId = e.StudentId,
            StudentName = $"{e.Student.FirstName} {e.Student.LastName}",
            ContractId = e.ContractId,
            ContractIdDisplay = e.ContractSession.ContractId
        }).ToList();
    }

    public async Task<List<GroupClassDto>> GetAllForAdminAsync()
    {
        var list = await _context.GroupClasses
            .AsNoTracking()
            .Include(g => g.Enrollments)
            .Include(g => g.Teacher).ThenInclude(t => t.User)
            .OrderBy(g => g.Name)
            .ToListAsync();
        return list.Select(g => MapToDto(g, g.Enrollments.Count, g.Teacher?.User != null ? $"{g.Teacher.User.FirstName} {g.Teacher.User.LastName}" : null)).ToList();
    }

    public async Task<GroupClassDto?> GetByIdForAdminAsync(int groupClassId)
    {
        var g = await _context.GroupClasses
            .AsNoTracking()
            .Include(x => x.Teacher).ThenInclude(t => t!.User)
            .FirstOrDefaultAsync(x => x.Id == groupClassId);
        if (g == null) return null;
        var count = await _context.GroupClassEnrollments.CountAsync(e => e.GroupClassId == groupClassId);
        return MapToDto(g, count, g.Teacher?.User != null ? $"{g.Teacher.User.FirstName} {g.Teacher.User.LastName}" : null);
    }

    public async Task<bool> UpdateByAdminAsync(int groupClassId, int teacherId, string name, bool isActive, string? zoomJoinUrl = null)
    {
        var g = await _context.GroupClasses.Include(x => x.Enrollments).FirstOrDefaultAsync(x => x.Id == groupClassId);
        if (g == null) return false;
        if (g.TeacherId != teacherId && g.Enrollments.Count > 0)
            throw new BusinessException("Cannot change assigned teacher when there are enrollments. Remove enrollments first or create a new group class.", "HAS_ENROLLMENTS");
        g.TeacherId = teacherId;
        if (!string.IsNullOrWhiteSpace(name)) g.Name = name.Trim();
        g.IsActive = isActive;
        g.ZoomJoinUrl = string.IsNullOrWhiteSpace(zoomJoinUrl) ? null : zoomJoinUrl.Trim();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<GroupClassEnrollmentDto>> GetEnrollmentsForAdminAsync(int groupClassId)
    {
        var list = await _context.GroupClassEnrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.ContractSession)
            .Where(e => e.GroupClassId == groupClassId)
            .OrderBy(e => e.Student!.FirstName)
            .ToListAsync();
        return list.Select(e => new GroupClassEnrollmentDto
        {
            Id = e.Id,
            GroupClassId = e.GroupClassId,
            StudentId = e.StudentId,
            StudentName = $"{e.Student!.FirstName} {e.Student.LastName}",
            ContractId = e.ContractId,
            ContractIdDisplay = e.ContractSession?.ContractId ?? ""
        }).ToList();
    }

    public async Task<bool> UnenrollByAdminAsync(int enrollmentId)
    {
        var e = await _context.GroupClassEnrollments.FirstOrDefaultAsync(x => x.Id == enrollmentId);
        if (e == null) return false;
        _context.GroupClassEnrollments.Remove(e);
        await _context.SaveChangesAsync();
        return true;
    }

    private static GroupClassDto MapToDto(GroupClass g, int enrolledCount, string? teacherName = null)
    {
        return new GroupClassDto
        {
            Id = g.Id,
            TeacherId = g.TeacherId,
            TeacherName = teacherName,
            Name = g.Name,
            IsActive = g.IsActive,
            ZoomJoinUrl = g.ZoomJoinUrl,
            CreatedAt = g.CreatedAt,
            EnrolledCount = enrolledCount
        };
    }
}
