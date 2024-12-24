using CS322_PZ_AnteaPrimorac5157.Models;

namespace CS322_PZ_AnteaPrimorac5157.Repositories
{
    public interface IConfessionRepository : IRepository<Confession>
    {
        Task<Confession?> GetByIdAsync(int id, bool includeComments = false);
        Task<IEnumerable<Confession>> GetTopConfessionsAsync(int count);
        Task IncrementLikesAsync(int id);
        Task DecrementLikesAsync(int id);
    }
}