using WebAppRazor.DAL.Models;

namespace WebAppRazor.BLL.Services
{
    public class ReviewResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int PointsEarned { get; set; }
    }

    public interface IMealReviewService
    {
        Task<ReviewResult> SubmitReviewAsync(int userId, int mealItemId, int rating, string comment);
        Task<List<MealReview>> GetReviewsByMealItemAsync(int mealItemId);
        Task<List<MealReview>> GetReviewsByUserAsync(int userId);
        Task<List<MealReview>> GetRecentReviewsAsync(int count = 20);
    }
}
