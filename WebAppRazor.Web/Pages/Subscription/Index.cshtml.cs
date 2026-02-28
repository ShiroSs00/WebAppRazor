using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppRazor.BLL.Services;
using WebAppRazor.DAL.Repositories;

namespace WebAppRazor.Web.Pages.Subscription
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUserRepository _userRepository;

        public IndexModel(ISubscriptionService subscriptionService, IUserRepository userRepository)
        {
            _subscriptionService = subscriptionService;
            _userRepository = userRepository;
        }

        public string CurrentTier { get; set; } = "Free";
        public DateTime? ExpiresAt { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var userId = GetUserId();
            CurrentTier = await _subscriptionService.GetUserTierAsync(userId);
            var user = await _userRepository.GetByIdAsync(userId);
            ExpiresAt = user?.SubscriptionExpiresAt;
        }

        public async Task<IActionResult> OnPostUpgradeAsync(string planType)
        {
            var userId = GetUserId();
            var result = await _subscriptionService.UpgradeToBasicPremiumAsync(userId, planType);

            if (result.Success)
            {
                SuccessMessage = $"Nâng cấp thành công! Gói {planType} sẽ hết hạn ngày {result.ExpiresAt?.ToString("dd/MM/yyyy")}.";
                CurrentTier = result.Tier;
                ExpiresAt = result.ExpiresAt;
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
                CurrentTier = await _subscriptionService.GetUserTierAsync(userId);
            }

            return Page();
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}
