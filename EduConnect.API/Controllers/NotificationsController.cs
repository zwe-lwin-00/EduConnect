using EduConnect.Shared.Extensions;
using EduConnect.Application.DTOs.Notifications;
using EduConnect.Application.Features.Notifications.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.API.Controllers;

[Authorize]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger) : base(logger)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Logger.WarningLog("GetMyNotifications: unauthorized");
            return Unauthorized();
        }
        var list = await _notificationService.GetForUserAsync(userId, unreadOnly);
        return Ok(list);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Logger.WarningLog("MarkAsRead: unauthorized");
            return Unauthorized();
        }
        var ok = await _notificationService.MarkAsReadAsync(id, userId);
        if (!ok)
        {
            Logger.WarningLog("MarkAsRead: notification not found");
            return NotFound();
        }
        Logger.InformationLog("Notification marked as read");
        return Ok(new { success = true });
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Logger.WarningLog("MarkAllAsRead: unauthorized");
            return Unauthorized();
        }
        var markedCount = await _notificationService.MarkAllAsReadAsync(userId);
        Logger.InformationLog($"Mark all as read: {markedCount} notification(s)");
        return Ok(new { success = true, markedCount });
    }
}
