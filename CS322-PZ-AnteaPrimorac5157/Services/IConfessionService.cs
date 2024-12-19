using CS322_PZ_AnteaPrimorac5157.ViewModels;
using CS322_PZ_AnteaPrimorac5157.Models;

namespace CS322_PZ_AnteaPrimorac5157.Services
{
    public interface IConfessionService
    {
        Task<Confession> CreateConfessionAsync(CreateConfessionViewModel model);
    }
}