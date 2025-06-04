using Microsoft.EntityFrameworkCore;
using Trabajo_Final.data;
using Trabajo_Final.DTOs.Porfolio;
using Trabajo_Final.models;
using Trabajo_Final.Services.Interfaces;

namespace Trabajo_Final.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly CryptoDbContext _context;
        private readonly ICriptoyaService _criptoyaService;

        public PortfolioService(CryptoDbContext context, ICriptoyaService criptoyaService)
        {
            _context = context;
            _criptoyaService = criptoyaService;
        }

        public async Task<PortfolioStatsResponse> GetPortfolioStatsAsync(int userId)
        {
            var balances = await GetPortfolioBalanceAsync(userId);

            var totalValue = balances.Sum(b => b.TotalValueArs ?? 0);
            var totalCryptocurrencies = balances.Count(b => b.CurrentBalance > 0);

            return new PortfolioStatsResponse
            {
                TotalCryptocurrencies = totalCryptocurrencies,
                TotalValueArs = totalValue,
                Holdings = balances
            };
        }

        public async Task<List<PortfolioBalanceResponse>> GetPortfolioBalanceAsync(int userId)
        {
            // Get user's portfolio balances
            var portfolioQuery = from c in _context.Cryptocurrencies
                                 join t in _context.Transactions on c.Id equals t.CryptocurrencyId into transactions
                                 from trans in transactions.DefaultIfEmpty()
                                 where c.IsActive
                                 group trans by new { c.Id, c.Symbol, c.Name, c.CriptoyaCode } into g
                                 select new
                                 {
                                     CryptocurrencyId = g.Key.Id,
                                     Symbol = g.Key.Symbol,
                                     Name = g.Key.Name,
                                     CriptoyaCode = g.Key.CriptoyaCode,
                                     CurrentBalance = g.Where(t => t != null && t.UserId == userId)
                                                    .Sum(t => t.TransactionType == TransactionType.BUY ? t.Quantity : -t.Quantity)
                                 };

            var portfolioBalances = await portfolioQuery.ToListAsync();

            // Filter only cryptocurrencies with positive balance
            var activeBalances = portfolioBalances.Where(p => p.CurrentBalance > 0).ToList();

            if (!activeBalances.Any())
                return new List<PortfolioBalanceResponse>();

            // Get current prices
            var coinCodes = activeBalances.Select(b => b.CriptoyaCode).ToList();
            var prices = await _criptoyaService.GetMultiplePricesAsync(coinCodes);

            // Get default exchange info
            var defaultExchange = await _context.Exchanges
                .FirstOrDefaultAsync(e => e.IsDefault && e.IsActive);

            var result = new List<PortfolioBalanceResponse>();

            foreach (var balance in activeBalances)
            {
                var currentPrice = prices.ContainsKey(balance.CriptoyaCode) ? prices[balance.CriptoyaCode] : (decimal?)null;
                var totalValue = currentPrice.HasValue ? balance.CurrentBalance * currentPrice.Value : (decimal?)null;

                result.Add(new PortfolioBalanceResponse
                {
                    CryptocurrencyId = balance.CryptocurrencyId,
                    Symbol = balance.Symbol,
                    Name = balance.Name,
                    CurrentBalance = balance.CurrentBalance,
                    CurrentPriceArs = currentPrice,
                    TotalValueArs = totalValue,
                    ExchangeName = defaultExchange?.Name ?? "N/A"
                });
            }

            return result.OrderByDescending(r => r.TotalValueArs ?? 0).ToList();
        }

        public async Task<bool> CanSellQuantityAsync(int userId, int cryptoId, decimal quantity)
        {
            var currentBalance = await _context.Transactions
                .Where(t => t.UserId == userId && t.CryptocurrencyId == cryptoId)
                .SumAsync(t => t.TransactionType == TransactionType.BUY ? t.Quantity : -t.Quantity);

            return currentBalance >= quantity;
        }
    }
}
