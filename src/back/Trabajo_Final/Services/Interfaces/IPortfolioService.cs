using Trabajo_Final.DTOs.Porfolio;

namespace Trabajo_Final.Services.Interfaces
{
    public interface IPortfolioService
    {
        Task<PortfolioStatsResponse> GetPortfolioStatsAsync(int userId);
        Task<List<PortfolioBalanceResponse>> GetPortfolioBalanceAsync(int userId);
        Task<bool> CanSellQuantityAsync(int userId, int cryptoId, decimal quantity);
    }
}
