using WebAppRazor.DAL.Models;

namespace WebAppRazor.BLL.Services
{
    public interface IProgressService
    {
        Task<bool> LogProgressAsync(int userId, double weight, string notes);
        Task<List<ProgressEntry>> GetProgressHistoryAsync(int userId);
        Task<ProgressEntry?> GetLatestProgressAsync(int userId);
    }
}
