namespace WebAppRazor.BLL.Services
{
    public class SubscriptionResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string Tier { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }

    public interface ISubscriptionService
    {
        Task<SubscriptionResult> UpgradeToBasicPremiumAsync(int userId, string planType);
        Task<string> GetUserTierAsync(int userId);
        Task<bool> IsPremiumAsync(int userId);
    }
}
