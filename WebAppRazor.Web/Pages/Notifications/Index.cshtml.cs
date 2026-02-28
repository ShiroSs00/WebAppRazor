using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppRazor.BLL.Services;
using WebAppRazor.DAL.Models;

namespace WebAppRazor.Web.Pages.Notifications
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly INotificationService _notificationService;

        public IndexModel(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public List<Notification> AllNotifications { get; set; } = new();
        public int UnreadCount { get; set; }

        public async Task OnGetAsync()
        {
            var userId = GetUserId();
            AllNotifications = await _notificationService.GetUserNotificationsAsync(userId);
            UnreadCount = await _notificationService.GetUnreadCountAsync(userId);
        }

        public async Task<IActionResult> OnPostMarkReadAsync(int notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkAllReadAsync()
        {
            var userId = GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateReminderAsync(string reminderType)
        {
            var userId = GetUserId();

            switch (reminderType)
            {
                case "Breakfast":
                case "Lunch":
                case "Dinner":
                case "Snack":
                    await _notificationService.CreateMealReminderAsync(userId, reminderType);
                    break;
                case "Shopping":
                    await _notificationService.CreateShoppingReminderAsync(userId);
                    break;
            }

            return RedirectToPage();
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}
