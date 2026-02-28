using WebAppRazor.DAL.Repositories;

namespace WebAppRazor.BLL.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUserRepository _userRepository;

        public SubscriptionService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<SubscriptionResult> UpgradeToBasicPremiumAsync(int userId, string planType)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new SubscriptionResult
                {
                    Success = false,
                    ErrorMessage = "Không tìm thấy người dùng."
                };
            }

            DateTime expiresAt = planType switch
            {
                "Weekly" => DateTime.UtcNow.AddDays(7),
                "Monthly" => DateTime.UtcNow.AddMonths(1),
                "Yearly" => DateTime.UtcNow.AddYears(1),
                _ => DateTime.UtcNow.AddMonths(1)
            };

            user.SubscriptionTier = "BasicPremium";
            user.SubscriptionExpiresAt = expiresAt;

            var success = await _userRepository.UpdateAsync(user);

            return new SubscriptionResult
            {
                Success = success,
                ErrorMessage = success ? null : "Không thể nâng cấp. Vui lòng thử lại.",
                Tier = "BasicPremium",
                ExpiresAt = expiresAt
            };
        }

        public async Task<string> GetUserTierAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return "Free";

            // Check if subscription expired
            if (user.SubscriptionTier == "BasicPremium" && user.SubscriptionExpiresAt.HasValue)
            {
                if (user.SubscriptionExpiresAt.Value < DateTime.UtcNow)
                {
                    user.SubscriptionTier = "Free";
                    user.SubscriptionExpiresAt = null;
                    await _userRepository.UpdateAsync(user);
                    return "Free";
                }
            }

            return user.SubscriptionTier;
        }

        public async Task<bool> IsPremiumAsync(int userId)
        {
            var tier = await GetUserTierAsync(userId);
            return tier == "BasicPremium";
        }
    }
}
