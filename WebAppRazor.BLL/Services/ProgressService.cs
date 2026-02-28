using WebAppRazor.DAL.Models;
using WebAppRazor.DAL.Repositories;

namespace WebAppRazor.BLL.Services
{
    public class ProgressService : IProgressService
    {
        private readonly IProgressRepository _progressRepository;
        private readonly IHealthProfileRepository _healthProfileRepository;
        private readonly IHealthProfileService _healthProfileService;

        public ProgressService(
            IProgressRepository progressRepository,
            IHealthProfileRepository healthProfileRepository,
            IHealthProfileService healthProfileService)
        {
            _progressRepository = progressRepository;
            _healthProfileRepository = healthProfileRepository;
            _healthProfileService = healthProfileService;
        }

        public async Task<bool> LogProgressAsync(int userId, double weight, string notes)
        {
            var latestProfile = await _healthProfileRepository.GetLatestByUserIdAsync(userId);

            double bmi = 0, bmr = 0, tdee = 0;

            if (latestProfile != null)
            {
                var metrics = _healthProfileService.CalculateMetrics(
                    latestProfile.Age,
                    latestProfile.Gender,
                    latestProfile.Height,
                    weight,
                    latestProfile.ActivityLevel,
                    latestProfile.Goal);

                bmi = metrics.BMI;
                bmr = metrics.BMR;
                tdee = metrics.TDEE;
            }

            var entry = new ProgressEntry
            {
                UserId = userId,
                Weight = weight,
                BMI = bmi,
                BMR = bmr,
                TDEE = tdee,
                Notes = notes,
                RecordedAt = DateTime.UtcNow
            };

            return await _progressRepository.CreateAsync(entry);
        }

        public async Task<List<ProgressEntry>> GetProgressHistoryAsync(int userId)
        {
            return await _progressRepository.GetByUserIdAsync(userId);
        }

        public async Task<ProgressEntry?> GetLatestProgressAsync(int userId)
        {
            return await _progressRepository.GetLatestByUserIdAsync(userId);
        }
    }
}
