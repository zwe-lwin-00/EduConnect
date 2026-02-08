using EduConnect.Application.Common.Interfaces;
using EduConnect.Application.DTOs.Notifications;
using EduConnect.Shared.Enums;

namespace EduConnect.Application.Features.Notifications.Interfaces;

public interface INotificationService : IService
{
    Task CreateForUserAsync(string userId, string title, string message, NotificationType type, string? relatedEntityType = null, int? relatedEntityId = null);
    Task<List<NotificationDto>> GetForUserAsync(string userId, bool unreadOnly = false);
    Task<bool> MarkAsReadAsync(int notificationId, string userId);
}
