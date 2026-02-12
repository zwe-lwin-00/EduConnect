using EduConnect.Application.DTOs.Notifications;
using EduConnect.Application.Features.Notifications.Interfaces;
using EduConnect.Domain.Entities;
using EduConnect.Infrastructure.Data;
using EduConnect.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduConnect.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public NotificationService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task CreateForUserAsync(string userId, string title, string message, NotificationType type, string? relatedEntityType = null, int? relatedEntityId = null)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    public async Task<List<NotificationDto>> GetForUserAsync(string userId, bool unreadOnly = false)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return new List<NotificationDto>();

        var query = _context.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId);
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);
        var list = await query.OrderByDescending(n => n.CreatedAt).Take(100).ToListAsync();
        var dtos = list.Select(MapToDto).ToList();

        if (user.Role == UserRole.Admin)
        {
            var now = DateTime.UtcNow.Date;
            var contractExpiringDays = _configuration.GetValue("App:ContractExpiringAlertDays", 14);
            var endWindow = now.AddDays(contractExpiringDays);
            var endingContracts = await _context.ContractSessions
                .AsNoTracking()
                .Include(c => c.Student)
                .Include(c => c.Teacher).ThenInclude(t => t!.User)
                .Where(c => c.Status == ContractStatus.Active && c.EndDate != null && c.EndDate >= now && c.EndDate <= endWindow)
                .OrderBy(c => c.EndDate)
                .Take(20)
                .ToListAsync();
            foreach (var c in endingContracts)
            {
                dtos.Add(new NotificationDto
                {
                    Id = -c.Id,
                    Title = "Contract ending soon",
                    Message = $"Contract {c.ContractId} ({c.Teacher?.User?.FullName} – {c.Student?.FullName}) ends on {c.EndDate:dd MMM yyyy}.",
                    Type = (int)NotificationType.ContractEndingSoon,
                    TypeName = "ContractEndingSoon",
                    RelatedEntityType = "Contract",
                    RelatedEntityId = c.Id,
                    IsRead = false,
                    CreatedAt = c.CreatedAt
                });
            }
        }

        if (user.Role == UserRole.Teacher)
        {
            var teacherProfile = await _context.TeacherProfiles.AsNoTracking().FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacherProfile != null)
            {
                var (todayStart, todayEnd) = (DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1));
                var sessionsToday = await _context.AttendanceLogs
                    .AsNoTracking()
                    .Include(a => a.ContractSession!).ThenInclude(c => c.Student)
                    .Where(a => a.ContractSession!.TeacherId == teacherProfile.Id && a.CheckInTime >= todayStart && a.CheckInTime < todayEnd)
                    .CountAsync();
                if (sessionsToday > 0)
                {
                    dtos.Insert(0, new NotificationDto
                    {
                        Id = -1,
                        Title = "Session reminder",
                        Message = $"You have {sessionsToday} session(s) today.",
                        Type = (int)NotificationType.SessionReminder,
                        TypeName = "SessionReminder",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        return dtos.OrderByDescending(d => d.CreatedAt).ToList();
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
    {
        if (notificationId < 0) return true;
        var n = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
        if (n == null) return false;
        n.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(string userId)
    {
        var toUpdate = await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in toUpdate)
            n.IsRead = true;
        if (toUpdate.Count > 0)
            await _context.SaveChangesAsync();
        return toUpdate.Count;
    }

    private static string TypeName(NotificationType t)
    {
        return t switch
        {
            NotificationType.HomeworkAssigned => "HomeworkAssigned",
            NotificationType.SessionCompleted => "SessionCompleted",
            NotificationType.GradeRecorded => "GradeRecorded",
            NotificationType.NewContract => "NewContract",
            NotificationType.SessionReminder => "SessionReminder",
            NotificationType.PendingVerification => "PendingVerification",
            NotificationType.ContractEndingSoon => "ContractEndingSoon",
            _ => t.ToString()
        };
    }

    private static NotificationDto MapToDto(Notification n)
    {
        return new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = (int)n.Type,
            TypeName = TypeName(n.Type),
            RelatedEntityType = n.RelatedEntityType,
            RelatedEntityId = n.RelatedEntityId,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}
