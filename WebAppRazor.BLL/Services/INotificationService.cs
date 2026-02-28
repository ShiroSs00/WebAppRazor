using WebAppRazor.DAL.Models;

namespace WebAppRazor.BLL.Services
{
    public interface INotificationService
    {
        Task<bool> CreateNotificationAsync(int userId, string title, string message, string type);
        Task<List<Notification>> GetUserNotificationsAsync(int userId);
        Task<List<Notification>> GetUnreadNotificationsAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task CreateMealReminderAsync(int userId, string mealType);
        Task CreateReviewReminderAsync(int userId, string mealName);
        Task CreateShoppingReminderAsync(int userId);
        Task CreateNewMenuNotificationAsync(int userId);
    }
}
