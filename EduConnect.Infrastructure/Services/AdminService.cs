using EduConnect.Application.Common.Exceptions;
using EduConnect.Application.Common.Models;
using EduConnect.Application.DTOs.Admin;
using EduConnect.Application.Features.Admin.Interfaces;
using EduConnect.Application.Features.Notifications.Interfaces;
using EduConnect.Domain.Entities;
using EduConnect.Infrastructure.Data;
using EduConnect.Infrastructure.Repositories;
using EduConnect.Shared.Enums;
using EduConnect.Shared.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EduConnect.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEncryptionService _encryptionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IEncryptionService encryptionService,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<AdminService> logger)
    {
        _context = context;
        _userManager = userManager;
        _encryptionService = encryptionService;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<OnboardTeacherResponse> OnboardTeacherAsync(OnboardTeacherRequest request, string adminUserId)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.WarningLog("OnboardTeacher: user with email already exists");
            throw new BusinessException("A user with this email already exists.", "USER_EXISTS");
        }

        // Create user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = UserRole.Teacher,
            MustChangePassword = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Generate random password - returned so admin can share with teacher (one-time only)
        var temporaryPassword = GenerateRandomPassword();
        var result = await _userManager.CreateAsync(user, temporaryPassword);

        if (!result.Succeeded)
        {
            throw new BusinessException(
                $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}",
                "USER_CREATION_FAILED");
        }

        // Encrypt NRC number
        var encryptedNrc = _encryptionService.Encrypt(request.NrcNumber);

        // Create teacher profile
        var teacherProfile = new TeacherProfile
        {
            UserId = user.Id,
            NrcNumber = encryptedNrc,
            EducationLevel = request.EducationLevel,
            HourlyRate = request.HourlyRate,
            Bio = request.Bio,
            Specializations = request.Specializations,
            VerificationStatus = VerificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.TeacherProfiles.Add(teacherProfile);
        await _unitOfWork.SaveChangesAsync();

        var teacherName = $"{user.FirstName} {user.LastName}";
        var adminIds = await _context.Users.Where(u => u.Role == UserRole.Admin).Select(u => u.Id).ToListAsync();
        foreach (var adminId in adminIds)
        {
            await _notificationService.CreateForUserAsync(adminId, "Pending teacher verification", $"Teacher {teacherName} is pending verification.", NotificationType.PendingVerification, "Teacher", teacherProfile.Id);
        }
        _logger.InformationLog("Teacher onboarded successfully");

        return new OnboardTeacherResponse
        {
            UserId = user.Id,
            TemporaryPassword = temporaryPassword
        };
    }

    public async Task<ResetTeacherPasswordResponse> ResetTeacherPasswordAsync(int teacherId)
    {
        var teacher = await _context.TeacherProfiles
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (teacher == null)
        {
            throw new NotFoundException("Teacher", teacherId);
        }

        var user = teacher.User;
        var newPassword = GenerateRandomPassword();

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            throw new BusinessException(
                $"Failed to reset password: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}",
                "PASSWORD_RESET_FAILED");
        }

        var addResult = await _userManager.AddPasswordAsync(user, newPassword);
        if (!addResult.Succeeded)
        {
            throw new BusinessException(
                $"Failed to set new password: {string.Join(", ", addResult.Errors.Select(e => e.Description))}",
                "PASSWORD_RESET_FAILED");
        }

        user.MustChangePassword = true;
        await _userManager.UpdateAsync(user);

        return new ResetTeacherPasswordResponse
        {
            Email = user.Email ?? string.Empty,
            TemporaryPassword = newPassword
        };
    }

    public async Task<bool> VerifyTeacherAsync(int teacherId)
    {
        var teacher = await _context.TeacherProfiles
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (teacher == null)
        {
            throw new NotFoundException("Teacher", teacherId);
        }

        teacher.VerificationStatus = VerificationStatus.Verified;
        teacher.VerifiedAt = DateTime.UtcNow;
        teacher.User.IsActive = true;

        await _unitOfWork.SaveChangesAsync();
        _logger.InformationLog("Teacher verified");
        return true;
    }

    public async Task<bool> RejectTeacherAsync(int teacherId, string reason)
    {
        var teacher = await _context.TeacherProfiles
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (teacher == null)
        {
            throw new NotFoundException("Teacher", teacherId);
        }

        teacher.VerificationStatus = VerificationStatus.Rejected;
        teacher.User.IsActive = false;

        await _unitOfWork.SaveChangesAsync();
        _logger.InformationLog("Teacher rejected");
        return true;
    }

    public async Task<List<TeacherDto>> GetTeachersAsync(string? searchTerm = null, int? verificationStatus = null, string? specializations = null)
    {
        var query = _context.TeacherProfiles.Include(t => t.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(t =>
                t.User.FirstName.Contains(term) ||
                t.User.LastName.Contains(term) ||
                (t.User.Email != null && t.User.Email.Contains(term)));
        }
        if (verificationStatus.HasValue)
            query = query.Where(t => (int)t.VerificationStatus == verificationStatus.Value);
        if (!string.IsNullOrWhiteSpace(specializations))
        {
            var spec = specializations.Trim();
            query = query.Where(t => t.Specializations != null && t.Specializations.Contains(spec));
        }
        var teachers = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return teachers.Select(t => MapToTeacherDto(t)).ToList();
    }

    public async Task<TeacherDto?> GetTeacherByIdAsync(int teacherId)
    {
        var teacher = await _context.TeacherProfiles
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        return teacher == null ? null : MapToTeacherDto(teacher);
    }

    public async Task<bool> UpdateTeacherAsync(int teacherId, UpdateTeacherRequest request)
    {
        var teacher = await _context.TeacherProfiles
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (teacher == null)
        {
            throw new NotFoundException("Teacher", teacherId);
        }

        teacher.User.FirstName = request.FirstName;
        teacher.User.LastName = request.LastName;
        teacher.User.PhoneNumber = request.PhoneNumber ?? string.Empty;
        teacher.EducationLevel = request.EducationLevel;
        teacher.HourlyRate = request.HourlyRate;
        teacher.Bio = request.Bio;
        teacher.Specializations = request.Specializations;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<ParentDto>> GetParentsAsync()
    {
        var parents = await _context.Users
            .Where(u => u.Role == UserRole.Parent)
            .Include(u => u.Students)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return parents.Select(p => MapToParentDto(p)).ToList();
    }

    public async Task<ParentDto?> GetParentByIdAsync(string parentId)
    {
        var parent = await _context.Users
            .Include(u => u.Students)
            .FirstOrDefaultAsync(u => u.Id == parentId && u.Role == UserRole.Parent);

        return parent == null ? null : MapToParentDto(parent);
    }

    public async Task<List<StudentDto>> GetStudentsAsync(string? parentId = null, int? gradeLevel = null)
    {
        var query = _context.Students
            .Include(s => s.Parent)
            .Include(s => s.ContractSessions)
            .AsQueryable();
        if (!string.IsNullOrEmpty(parentId))
            query = query.Where(s => s.ParentId == parentId);
        if (gradeLevel.HasValue && gradeLevel.Value >= 1 && gradeLevel.Value <= 4)
            query = query.Where(s => (int)s.GradeLevel == gradeLevel.Value);
        var students = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        return students.Select(s => MapToStudentDto(s)).ToList();
    }

    public async Task<List<StudentDto>> GetStudentsByParentAsync(string parentId)
    {
        return await GetStudentsAsync(parentId, null);
    }

    public async Task<PagedResult<TeacherDto>> GetTeachersPagedAsync(PagedRequest request, int? verificationStatus = null, string? specializations = null)
    {
        var query = _context.TeacherProfiles
            .Include(t => t.User)
            .AsQueryable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(t =>
                t.User.FirstName.Contains(request.SearchTerm) ||
                t.User.LastName.Contains(request.SearchTerm) ||
                (t.User.Email != null && t.User.Email.Contains(request.SearchTerm)));
        }
        if (verificationStatus.HasValue)
            query = query.Where(t => (int)t.VerificationStatus == verificationStatus.Value);
        if (!string.IsNullOrWhiteSpace(specializations))
            query = query.Where(t => t.Specializations != null && t.Specializations.Contains(specializations.Trim()));

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(t => t.User.LastName)
                : query.OrderBy(t => t.User.LastName),
            "email" => request.SortDescending
                ? query.OrderByDescending(t => t.User.Email)
                : query.OrderBy(t => t.User.Email),
            "created" => request.SortDescending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<TeacherDto>
        {
            Items = items.Select(MapToTeacherDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<PagedResult<ParentDto>> GetParentsPagedAsync(PagedRequest request)
    {
        var query = _context.Users
            .Where(u => u.Role == UserRole.Parent)
            .Include(u => u.Students)
            .AsQueryable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(u =>
                (u.FirstName != null && u.FirstName.Contains(term)) ||
                (u.LastName != null && u.LastName.Contains(term)) ||
                (u.Email != null && u.Email.Contains(term)));
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(u => u.LastName)
                : query.OrderBy(u => u.LastName),
            "email" => request.SortDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<ParentDto>
        {
            Items = items.Select(MapToParentDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    // ——— Dashboard ———
    public async Task<DashboardDto> GetDashboardAsync()
    {
        var myanmarNow = MyanmarTimeHelper.GetMyanmarNow();
        var today = myanmarNow.Date;
        var startOfMonthMyanmar = new DateTime(myanmarNow.Year, myanmarNow.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var endOfMonthMyanmar = startOfMonthMyanmar.AddMonths(1);
        var startOfMonth = TimeZoneInfo.ConvertTimeToUtc(startOfMonthMyanmar, MyanmarTimeHelper.MyanmarTimeZone);
        var endOfMonth = TimeZoneInfo.ConvertTimeToUtc(endOfMonthMyanmar, MyanmarTimeHelper.MyanmarTimeZone);

        // Alerts: low remaining hours (<2)
        var lowHoursContracts = await _context.ContractSessions
            .Include(c => c.Teacher).ThenInclude(t => t!.User)
            .Include(c => c.Student)
            .Where(c => c.Status == ContractStatus.Active && c.RemainingHours < 2 && c.RemainingHours >= 0)
            .ToListAsync();
        var alerts = lowHoursContracts.Select(c => new DashboardAlertDto
        {
            Type = "LowHours",
            Message = $"Remaining hours < 2 for {c.Student.FirstName} {c.Student.LastName} (Contract {c.ContractId})",
            EntityId = c.ContractId,
            EntityName = $"{c.Student.FirstName} {c.Student.LastName}"
        }).ToList();

        // Alerts: contracts ending in next 14 days
        var expiringEnd = today.AddDays(14);
        var expiring = await _context.ContractSessions
            .Include(c => c.Student)
            .Include(c => c.Teacher).ThenInclude(t => t!.User)
            .Where(c => c.Status == ContractStatus.Active && c.EndDate.HasValue && c.EndDate.Value.Date <= expiringEnd && c.EndDate.Value.Date >= today)
            .OrderBy(c => c.EndDate)
            .ToListAsync();
        foreach (var c in expiring)
            alerts.Add(new DashboardAlertDto
            {
                Type = "ContractExpiring",
                Message = $"Contract {c.ContractId} ends on {c.EndDate!.Value:yyyy-MM-dd} – {c.Student.FirstName} {c.Student.LastName}",
                EntityId = c.ContractId,
                EntityName = c.Student.FirstName + " " + c.Student.LastName
            });

        // Today's sessions (today = Myanmar date)
        var (todayStartUtc, todayEndUtc) = MyanmarTimeHelper.GetTodayUtcRange();
        var todayLogs = await _context.AttendanceLogs
            .Include(a => a.ContractSession)
                .ThenInclude(c => c!.Teacher)
                .ThenInclude(t => t!.User)
            .Include(a => a.ContractSession!)
                .ThenInclude(c => c.Student)
            .Where(a => (a.CheckInTime >= todayStartUtc && a.CheckInTime < todayEndUtc) || (a.CheckOutTime == null && a.CreatedAt >= todayStartUtc && a.CreatedAt < todayEndUtc))
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();
        var todaySessions = todayLogs.Select(a => new TodaySessionDto
        {
            Id = a.Id,
            ContractId = a.ContractSession?.ContractId ?? "",
            TeacherName = a.ContractSession?.Teacher != null ? $"{a.ContractSession.Teacher.User.FirstName} {a.ContractSession.Teacher.User.LastName}" : "",
            StudentName = a.ContractSession?.Student != null ? $"{a.ContractSession.Student.FirstName} {a.ContractSession.Student.LastName}" : "",
            Status = a.Status.ToString(),
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime
        }).ToList();

        // Pending: unverified teachers + contracts not yet active (if we had Pending status)
        var pendingTeachers = await _context.TeacherProfiles.CountAsync(t => t.VerificationStatus == VerificationStatus.Pending);
        var pendingActions = pendingTeachers;

        // Revenue snapshot (month): hours consumed * margin; for simplicity use sum of (HoursUsed * Teacher.HourlyRate) as proxy for revenue
        var monthLogs = await _context.AttendanceLogs
            .Include(a => a.ContractSession!)
                .ThenInclude(c => c.Teacher)
            .Where(a => a.CheckInTime >= startOfMonth && a.CheckInTime < endOfMonth && a.CheckOutTime != null)
            .ToListAsync();
        decimal revenue = 0;
        decimal hoursConsumed = 0;
        foreach (var log in monthLogs)
        {
            if (log.ContractSession?.Teacher != null)
            {
                revenue += log.HoursUsed * log.ContractSession.Teacher.HourlyRate;
                hoursConsumed += log.HoursUsed;
            }
        }

        return new DashboardDto
        {
            Alerts = alerts,
            TodaySessions = todaySessions,
            PendingActionsCount = pendingActions,
            RevenueSnapshot = new RevenueSnapshotDto
            {
                RevenueThisMonth = revenue,
                SessionsDeliveredThisMonth = monthLogs.Count,
                HoursConsumedThisMonth = hoursConsumed
            }
        };
    }

    // ——— Create Parent / Student ———
    public async Task<CreateParentResponse> CreateParentAsync(CreateParentRequest request, string adminUserId)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            throw new BusinessException("A user with this email already exists.", "USER_EXISTS");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = UserRole.Parent,
            MustChangePassword = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var password = GenerateRandomPassword();
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new BusinessException(string.Join(", ", result.Errors.Select(e => e.Description)), "USER_CREATION_FAILED");
        return new CreateParentResponse
        {
            UserId = user.Id,
            Email = user.Email ?? request.Email,
            TemporaryPassword = password
        };
    }

    public async Task<int> CreateStudentAsync(CreateStudentRequest request, string adminUserId)
    {
        var parent = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ParentId && u.Role == UserRole.Parent);
        if (parent == null)
            throw new NotFoundException("Parent", request.ParentId);

        if (request.GradeLevel < 1 || request.GradeLevel > 4)
            throw new BusinessException("GradeLevel must be P1–P4 (1–4).", "INVALID_GRADE");

        var student = new Student
        {
            ParentId = request.ParentId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            GradeLevel = (GradeLevel)request.GradeLevel,
            DateOfBirth = request.DateOfBirth,
            SpecialNeeds = request.SpecialNeeds,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Students.Add(student);
        await _unitOfWork.SaveChangesAsync();
        return student.Id;
    }

    public async Task<bool> SetTeacherActiveAsync(int teacherId, bool isActive)
    {
        var teacher = await _context.TeacherProfiles.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == teacherId);
        if (teacher == null)
            throw new NotFoundException("Teacher", teacherId);
        teacher.User.IsActive = isActive;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ——— Contracts ———
    public async Task<List<ContractDto>> GetContractsAsync(int? teacherId = null, int? studentId = null, int? status = null)
    {
        var query = _context.ContractSessions
            .Include(c => c.Teacher).ThenInclude(t => t!.User)
            .Include(c => c.Student).ThenInclude(s => s!.Parent)
            .AsQueryable();
        if (teacherId.HasValue)
            query = query.Where(c => c.TeacherId == teacherId);
        if (studentId.HasValue)
            query = query.Where(c => c.StudentId == studentId);
        if (status.HasValue)
            query = query.Where(c => (int)c.Status == status.Value);
        var list = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return list.Select(MapToContractDto).ToList();
    }

    public async Task<ContractDto?> GetContractByIdAsync(int id)
    {
        var c = await _context.ContractSessions
            .Include(x => x.Teacher).ThenInclude(t => t!.User)
            .Include(x => x.Student).ThenInclude(s => s!.Parent)
            .FirstOrDefaultAsync(x => x.Id == id);
        return c == null ? null : MapToContractDto(c);
    }

    public async Task<ContractDto> CreateContractAsync(CreateContractRequest request, string adminUserId)
    {
        var teacher = await _context.TeacherProfiles.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == request.TeacherId);
        if (teacher == null)
            throw new NotFoundException("Teacher", request.TeacherId);
        var student = await _context.Students.Include(s => s.Parent).FirstOrDefaultAsync(s => s.Id == request.StudentId);
        if (student == null)
            throw new NotFoundException("Student", request.StudentId);

        var contractId = "C-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var contract = new ContractSession
        {
            ContractId = contractId,
            TeacherId = request.TeacherId,
            StudentId = request.StudentId,
            PackageHours = request.PackageHours,
            RemainingHours = request.PackageHours,
            Status = ContractStatus.Active,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedBy = adminUserId,
            CreatedAt = DateTime.UtcNow
        };
        _context.ContractSessions.Add(contract);
        await _unitOfWork.SaveChangesAsync();
        _logger.InformationLog("Contract created");

        var teacherUserId = teacher.UserId;
        await _notificationService.CreateForUserAsync(teacherUserId, "New contract", $"New contract {contract.ContractId} with student {student.FirstName} {student.LastName}.", NotificationType.NewContract, "Contract", contract.Id);

        if (contract.EndDate.HasValue)
        {
            var daysUntilEnd = (contract.EndDate.Value - DateTime.UtcNow.Date).TotalDays;
            if (daysUntilEnd >= 0 && daysUntilEnd <= 14)
            {
                var adminIds = await _context.Users.Where(u => u.Role == UserRole.Admin).Select(u => u.Id).ToListAsync();
                foreach (var adminId in adminIds)
                {
                    await _notificationService.CreateForUserAsync(adminId, "Contract ending soon", $"Contract {contract.ContractId} ends on {contract.EndDate:dd MMM yyyy}.", NotificationType.ContractEndingSoon, "Contract", contract.Id);
                }
            }
        }
        return MapToContractDto(contract);
    }

    public async Task<bool> ActivateContractAsync(int id)
    {
        var c = await _context.ContractSessions.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null)
            throw new NotFoundException("Contract", id);
        c.Status = ContractStatus.Active;
        await _unitOfWork.SaveChangesAsync();
        _logger.InformationLog("Contract activated");
        return true;
    }

    public async Task<bool> CancelContractAsync(int id)
    {
        var c = await _context.ContractSessions.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null)
            throw new NotFoundException("Contract", id);
        c.Status = ContractStatus.Cancelled;
        await _unitOfWork.SaveChangesAsync();
        _logger.InformationLog("Contract cancelled");
        return true;
    }

    private ContractDto MapToContractDto(ContractSession c)
    {
        return new ContractDto
        {
            Id = c.Id,
            ContractId = c.ContractId,
            TeacherId = c.TeacherId,
            TeacherName = c.Teacher != null ? $"{c.Teacher.User.FirstName} {c.Teacher.User.LastName}" : "",
            StudentId = c.StudentId,
            StudentName = c.Student != null ? $"{c.Student.FirstName} {c.Student.LastName}" : "",
            PackageHours = c.PackageHours,
            RemainingHours = c.RemainingHours,
            Status = (int)c.Status,
            StatusName = c.Status.ToString(),
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            CreatedAt = c.CreatedAt
        };
    }

    // ——— Attendance oversight ———
    public async Task<List<TodaySessionDto>> GetTodaySessionsAsync()
    {
        var dto = await GetDashboardAsync();
        return dto.TodaySessions;
    }

    public async Task<bool> OverrideCheckInAsync(int attendanceLogId, string adminUserId)
    {
        var log = await _context.AttendanceLogs
            .Include(a => a.ContractSession)
            .FirstOrDefaultAsync(a => a.Id == attendanceLogId);
        if (log == null)
            throw new NotFoundException("AttendanceLog", attendanceLogId);
        log.CheckInTime = DateTime.UtcNow;
        log.Status = SessionStatus.InProgress;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> OverrideCheckOutAsync(int attendanceLogId, string adminUserId)
    {
        var log = await _context.AttendanceLogs
            .Include(a => a.ContractSession)
            .FirstOrDefaultAsync(a => a.Id == attendanceLogId);
        if (log == null)
            throw new NotFoundException("AttendanceLog", attendanceLogId);
        var now = DateTime.UtcNow;
        log.CheckOutTime = now;
        log.HoursUsed = (decimal)(now - log.CheckInTime).TotalHours;
        log.Status = SessionStatus.Completed;
        if (log.ContractSession != null)
        {
            log.ContractSession.RemainingHours = Math.Max(0, log.ContractSession.RemainingHours - (int)Math.Ceiling(log.HoursUsed));
        }
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AdjustSessionHoursAsync(int attendanceLogId, AdjustHoursRequest request, string adminUserId)
    {
        var log = await _context.AttendanceLogs
            .Include(a => a.ContractSession)
            .FirstOrDefaultAsync(a => a.Id == attendanceLogId);
        if (log == null)
            throw new NotFoundException("AttendanceLog", attendanceLogId);
        var oldHours = (int)Math.Ceiling(log.HoursUsed);
        var newHours = (int)Math.Ceiling(request.Hours);
        log.HoursUsed = request.Hours;
        if (log.ContractSession != null)
        {
            // Give back old hours, deduct new hours from contract
            log.ContractSession.RemainingHours = Math.Max(0, log.ContractSession.RemainingHours + oldHours - newHours);
        }
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ——— Wallet ———
    public async Task<bool> CreditStudentHoursAsync(int studentId, int contractId, WalletAdjustRequest request, string adminUserId)
    {
        var c = await _context.ContractSessions.FirstOrDefaultAsync(x => x.StudentId == studentId && x.Id == contractId && x.Status == ContractStatus.Active);
        if (c == null)
            throw new NotFoundException("Contract", contractId);
        c.RemainingHours += (int)Math.Ceiling(request.Hours);
        await _unitOfWork.SaveChangesAsync();
        _logger.InformationLog("Wallet credited");
        return true;
    }

    public async Task<bool> DeductStudentHoursAsync(int studentId, int contractId, WalletAdjustRequest request, string adminUserId)
    {
        var c = await _context.ContractSessions.FirstOrDefaultAsync(x => x.StudentId == studentId && x.Id == contractId && x.Status == ContractStatus.Active);
        if (c == null)
            throw new NotFoundException("Contract", contractId);
        c.RemainingHours = Math.Max(0, c.RemainingHours - (int)Math.Ceiling(request.Hours));
        await _unitOfWork.SaveChangesAsync();
        _logger.InformationLog("Wallet deducted");
        return true;
    }

    public async Task<bool> SetStudentActiveAsync(int studentId, bool isActive)
    {
        var s = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId);
        if (s == null)
            throw new NotFoundException("Student", studentId);
        s.IsActive = isActive;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ——— Reports ———
    public async Task<DailyReportDto> GetDailyReportAsync(DateTime date)
    {
        // date is interpreted as calendar date in Myanmar
        var (startUtc, endUtc) = MyanmarTimeHelper.GetUtcRangeForMyanmarDate(date);
        var logs = await _context.AttendanceLogs
            .Where(a => a.CheckInTime >= startUtc && a.CheckInTime < endUtc && a.CheckOutTime != null)
            .ToListAsync();
        var hours = logs.Sum(a => a.HoursUsed);
        return new DailyReportDto
        {
            Date = date.Date,
            SessionsDelivered = logs.Count,
            HoursConsumed = hours
        };
    }

    public async Task<MonthlyReportDto> GetMonthlyReportAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);
        var logs = await _context.AttendanceLogs
            .Include(a => a.ContractSession!)
                .ThenInclude(c => c.Teacher)
                .ThenInclude(t => t!.User)
            .Where(a => a.CheckInTime >= start && a.CheckInTime < end && a.CheckOutTime != null)
            .ToListAsync();
        decimal revenue = 0;
        foreach (var log in logs.Where(l => l.ContractSession?.Teacher != null))
            revenue += log.HoursUsed * log.ContractSession!.Teacher!.HourlyRate;

        var byTeacher = logs
            .Where(l => l.ContractSession?.Teacher != null)
            .GroupBy(l => l.ContractSession!.TeacherId)
            .Select(g => new TeacherUtilizationDto
            {
                TeacherId = g.Key,
                TeacherName = g.First().ContractSession!.Teacher!.User.FirstName + " " + g.First().ContractSession!.Teacher!.User.LastName,
                HoursDelivered = g.Sum(x => x.HoursUsed),
                SessionsCount = g.Count()
            }).ToList();

        return new MonthlyReportDto
        {
            Year = year,
            Month = month,
            Revenue = revenue,
            SessionsDelivered = logs.Count,
            HoursConsumed = logs.Sum(a => a.HoursUsed),
            TeacherUtilization = byTeacher
        };
    }

    private TeacherDto MapToTeacherDto(TeacherProfile teacher)
    {
        return new TeacherDto
        {
            Id = teacher.Id,
            UserId = teacher.UserId,
            Email = teacher.User.Email ?? string.Empty,
            FirstName = teacher.User.FirstName,
            LastName = teacher.User.LastName,
            PhoneNumber = teacher.User.PhoneNumber ?? string.Empty,
            EducationLevel = teacher.EducationLevel,
            HourlyRate = teacher.HourlyRate,
            VerificationStatus = (int)teacher.VerificationStatus,
            VerificationStatusName = teacher.VerificationStatus.ToString(),
            Bio = teacher.Bio,
            Specializations = teacher.Specializations,
            CreatedAt = teacher.CreatedAt,
            VerifiedAt = teacher.VerifiedAt,
            IsActive = teacher.User.IsActive
        };
    }

    private ParentDto MapToParentDto(ApplicationUser parent)
    {
        return new ParentDto
        {
            Id = parent.Id,
            Email = parent.Email ?? string.Empty,
            FirstName = parent.FirstName,
            LastName = parent.LastName,
            PhoneNumber = parent.PhoneNumber ?? string.Empty,
            StudentCount = parent.Students.Count,
            CreatedAt = parent.CreatedAt,
            IsActive = parent.IsActive
        };
    }

    private StudentDto MapToStudentDto(Student student)
    {
        return new StudentDto
        {
            Id = student.Id,
            ParentId = student.ParentId,
            ParentName = $"{student.Parent.FirstName} {student.Parent.LastName}",
            FirstName = student.FirstName,
            LastName = student.LastName,
            GradeLevel = (int)student.GradeLevel,
            GradeLevelName = student.GradeLevel.ToString(),
            DateOfBirth = student.DateOfBirth,
            SpecialNeeds = student.SpecialNeeds,
            WalletBalance = student.ContractSessions
                .Where(c => c.Status == ContractStatus.Active)
                .Sum(c => c.RemainingHours),
            CreatedAt = student.CreatedAt,
            IsActive = student.IsActive
        };
    }

    private string GenerateRandomPassword(int length = 12)
    {
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string upper = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        const string all = lower + upper + digits + special;
        var random = new Random();
        // Ensure at least one of each required type (Identity: lowercase, uppercase, digit, non-alphanumeric)
        var password = new char[length];
        password[0] = lower[random.Next(lower.Length)];
        password[1] = upper[random.Next(upper.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];
        for (int i = 4; i < length; i++)
            password[i] = all[random.Next(all.Length)];
        // Shuffle so required chars aren't always at the start
        for (int i = length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }
        return new string(password);
    }
}
