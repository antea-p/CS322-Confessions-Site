using CS322_PZ_AnteaPrimorac5157.ViewModels;
using CS322_PZ_AnteaPrimorac5157.Models;

namespace CS322_PZ_AnteaPrimorac5157.Services
{
    public interface IConfessionService
    {
        Task<IEnumerable<Confession>> GetConfessionsAsync(bool orderByLikes = false);
        Task<Confession?> GetConfessionAsync(int id, bool includeComments = false);
        Task<Confession> CreateConfessionAsync(CreateConfessionViewModel model);
        Task DeleteConfessionAsync(int id);
        Task DeleteCommentAsync(int confessionId, int commentId);
        Task IncrementLikesAsync(int id);
        Task DecrementLikesAsync(int id);
    }
}