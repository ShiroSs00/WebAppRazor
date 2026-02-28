using WebAppRazor.DAL.Models;
using WebAppRazor.DAL.Repositories;

namespace WebAppRazor.BLL.Services
{
    public class MealReviewService : IMealReviewService
    {
        private readonly IMealReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;

        public MealReviewService(IMealReviewRepository reviewRepository, IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
        }

        public async Task<ReviewResult> SubmitReviewAsync(int userId, int mealItemId, int rating, string comment)
        {
            int points = rating >= 4 ? 15 : 10; // Bonus points for high ratings

            var review = new MealReview
            {
                UserId = userId,
                MealItemId = mealItemId,
                Rating = rating,
                Comment = comment,
                PointsEarned = points,
                CreatedAt = DateTime.UtcNow
            };

            var success = await _reviewRepository.CreateAsync(review);

            if (success)
            {
                // Update user review points
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    user.ReviewPoints += points;
                    await _userRepository.UpdateAsync(user);
                }
            }

            return new ReviewResult
            {
                Success = success,
                ErrorMessage = success ? null : "Không thể gửi đánh giá. Vui lòng thử lại.",
                PointsEarned = success ? points : 0
            };
        }

        public async Task<List<MealReview>> GetReviewsByMealItemAsync(int mealItemId)
        {
            return await _reviewRepository.GetByMealItemIdAsync(mealItemId);
        }

        public async Task<List<MealReview>> GetReviewsByUserAsync(int userId)
        {
            return await _reviewRepository.GetByUserIdAsync(userId);
        }

        public async Task<List<MealReview>> GetRecentReviewsAsync(int count = 20)
        {
            return await _reviewRepository.GetAllRecentAsync(count);
        }
    }
}
