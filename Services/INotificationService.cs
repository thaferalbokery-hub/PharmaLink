using PharmaLink.Models;

namespace PharmaLink.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, string title, string message, string? type = null, string? actionUrl = null);
    Task<List<Notification>> GetUserNotificationsAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task DeleteNotificationAsync(int notificationId);
}