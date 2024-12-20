using CS322_PZ_AnteaPrimorac5157.ViewModels;
using CS322_PZ_AnteaPrimorac5157.Models;

namespace CS322_PZ_AnteaPrimorac5157.Services
{
    public interface IConfessionService
    {
        Task<IEnumerable<Confession>> GetConfessionsAsync(bool orderByLikes = false);
        Task<Confession?> GetConfessionAsync(int id);
        Task<Confession> CreateConfessionAsync(CreateConfessionViewModel model);
        Task DeleteConfessionAsync(int id);
    }
}