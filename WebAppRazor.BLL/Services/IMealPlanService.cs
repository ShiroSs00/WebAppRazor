using WebAppRazor.DAL.Models;

namespace WebAppRazor.BLL.Services
{
    public interface IMealPlanService
    {
        Task<MealPlan> GenerateMenuAsync(int userId, double targetCalories, bool isPremium);
        Task<List<MealPlan>> GetUserPlansAsync(int userId);
        Task<MealPlan?> GetPlanWithItemsAsync(int planId);
        Task<bool> DeletePlanAsync(int planId);
    }
}
