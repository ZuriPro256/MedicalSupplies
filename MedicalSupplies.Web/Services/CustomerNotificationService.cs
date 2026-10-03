using System.Security.Claims;
using MedicalSupplies.Core.Entities;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Services;

public interface ICustomerNotificationService
{
    Task CreateAsync(
        string userId,
        string notificationType,
        string title,
        string message,
        string? url = null);

    Task<int> GetUnreadCountAsync(ClaimsPrincipal user);

    Task<List<Notification>> GetForCurrentUserAsync(ClaimsPrincipal user);

    Task<bool> MarkAsReadAsync(
        int notificationId,
        ClaimsPrincipal user);

    Task<int> MarkAllAsReadAsync(ClaimsPrincipal user);
}

public class CustomerNotificationService : ICustomerNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomerNotificationService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task CreateAsync(
        string userId,
        string notificationType,
        string title,
        string message,
        string? url = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return;

        var notification = new Notification
        {
            UserId = userId,
            NotificationType = notificationType,
            Title = title.Trim(),
            Message = message.Trim(),
            Url = string.IsNullOrWhiteSpace(url) ? null : url.Trim(),
            IsRead = false,
            CreatedDate = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);

        if (string.IsNullOrWhiteSpace(userId))
            return 0;

        return await _context.Notifications
            .CountAsync(n =>
                n.UserId == userId &&
                !n.IsRead);
    }

    public async Task<List<Notification>> GetForCurrentUserAsync(
        ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);

        if (string.IsNullOrWhiteSpace(userId))
            return new List<Notification>();

        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(
        int notificationId,
        ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);

        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.NotificationId == notificationId &&
                n.UserId == userId);

        if (notification is null)
            return false;

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(
        ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);

        if (string.IsNullOrWhiteSpace(userId))
            return 0;

        var unread = await _context.Notifications
            .Where(n =>
                n.UserId == userId &&
                !n.IsRead)
            .ToListAsync();

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }

        if (unread.Count > 0)
            await _context.SaveChangesAsync();

        return unread.Count;
    }
}
