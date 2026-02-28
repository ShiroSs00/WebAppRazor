using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppRazor.BLL.Services;
using WebAppRazor.DAL.Models;
using WebAppRazor.DAL.Repositories;

namespace WebAppRazor.Web.Pages.Reviews
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMealReviewService _reviewService;
        private readonly IUserRepository _userRepository;

        public IndexModel(IMealReviewService reviewService, IUserRepository userRepository)
        {
            _reviewService = reviewService;
            _userRepository = userRepository;
        }

        public List<MealReview> MyReviews { get; set; } = new();
        public List<MealReview> RecentReviews { get; set; } = new();
        public int UserPoints { get; set; }

        public async Task OnGetAsync()
        {
            var userId = GetUserId();
            MyReviews = await _reviewService.GetReviewsByUserAsync(userId);
            RecentReviews = await _reviewService.GetRecentReviewsAsync(20);

            var user = await _userRepository.GetByIdAsync(userId);
            UserPoints = user?.ReviewPoints ?? 0;
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}
