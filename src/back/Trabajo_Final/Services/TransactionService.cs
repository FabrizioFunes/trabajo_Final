using Microsoft.EntityFrameworkCore;
using Trabajo_Final.data;
using Trabajo_Final.DTOs.Transaction;
using Trabajo_Final.models;
using Trabajo_Final.Services.Interfaces;

namespace Trabajo_Final.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly CryptoDbContext _context;
        private readonly IPortfolioService _portfolioService;

        public TransactionService(CryptoDbContext context, IPortfolioService portfolioService)
        {
            _context = context;
            _portfolioService = portfolioService;
        }

        public async Task<TransactionResponse?> CreateTransactionAsync(int userId, CreateTransactionRequest request)
        {
            // Validate cryptocurrency exists
            var crypto = await _context.Cryptocurrencies.FindAsync(request.CryptocurrencyId);
            if (crypto == null) return null;

            // For SELL transactions, check if user has enough balance
            if (request.TransactionType == TransactionType.SELL)
            {
                var canSell = await _portfolioService.CanSellQuantityAsync(userId, request.CryptocurrencyId, request.Quantity);
                if (!canSell) return null;
            }

            var transaction = new Transaction
            {
                UserId = userId,
                TransactionType = request.TransactionType,
                CryptocurrencyId = request.CryptocurrencyId,
                Quantity = request.Quantity,
                PriceArs = request.PriceArs,
                TotalAmountArs = request.TotalAmountArs,
                TransactionDate = request.TransactionDate,
                Notes = request.Notes
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return new TransactionResponse
            {
                Id = transaction.Id,
                TransactionType = transaction.TransactionType,
                CryptocurrencySymbol = crypto.Symbol,
                CryptocurrencyName = crypto.Name,
                Quantity = transaction.Quantity,
                PriceArs = transaction.PriceArs,
                TotalAmountArs = transaction.TotalAmountArs,
                TransactionDate = transaction.TransactionDate,
                Notes = transaction.Notes,
                CreatedAt = transaction.CreatedAt
            };
        }

        public async Task<List<TransactionResponse>> GetTransactionHistoryAsync(int userId, int? cryptoId = null, int page = 1, int pageSize = 50)
        {
            var query = _context.Transactions
                .Include(t => t.Cryptocurrency)
                .Where(t => t.UserId == userId);

            if (cryptoId.HasValue)
            {
                query = query.Where(t => t.CryptocurrencyId == cryptoId.Value);
            }

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionResponse
                {
                    Id = t.Id,
                    TransactionType = t.TransactionType,
                    CryptocurrencySymbol = t.Cryptocurrency.Symbol,
                    CryptocurrencyName = t.Cryptocurrency.Name,
                    Quantity = t.Quantity,
                    PriceArs = t.PriceArs,
                    TotalAmountArs = t.TotalAmountArs,
                    TransactionDate = t.TransactionDate,
                    Notes = t.Notes,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return transactions;
        }

        public async Task<TransactionResponse?> GetTransactionByIdAsync(int userId, int transactionId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Cryptocurrency)
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

            if (transaction == null) return null;

            return new TransactionResponse
            {
                Id = transaction.Id,
                TransactionType = transaction.TransactionType,
                CryptocurrencySymbol = transaction.Cryptocurrency.Symbol,
                CryptocurrencyName = transaction.Cryptocurrency.Name,
                Quantity = transaction.Quantity,
                PriceArs = transaction.PriceArs,
                TotalAmountArs = transaction.TotalAmountArs,
                TransactionDate = transaction.TransactionDate,
                Notes = transaction.Notes,
                CreatedAt = transaction.CreatedAt
            };
        }

        public async Task<bool> DeleteTransactionAsync(int userId, int transactionId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

            if (transaction == null) return false;

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TransactionResponse?> UpdateTransactionAsync(int userId, int transactionId, CreateTransactionRequest request)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Cryptocurrency)
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

            if (transaction == null) return null;

            // Validate cryptocurrency exists
            var crypto = await _context.Cryptocurrencies.FindAsync(request.CryptocurrencyId);
            if (crypto == null) return null;

            transaction.TransactionType = request.TransactionType;
            transaction.CryptocurrencyId = request.CryptocurrencyId;
            transaction.Quantity = request.Quantity;
            transaction.PriceArs = request.PriceArs;
            transaction.TotalAmountArs = request.TotalAmountArs;
            transaction.TransactionDate = request.TransactionDate;
            transaction.Notes = request.Notes;
            transaction.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new TransactionResponse
            {
                Id = transaction.Id,
                TransactionType = transaction.TransactionType,
                CryptocurrencySymbol = crypto.Symbol,
                CryptocurrencyName = crypto.Name,
                Quantity = transaction.Quantity,
                PriceArs = transaction.PriceArs,
                TotalAmountArs = transaction.TotalAmountArs,
                TransactionDate = transaction.TransactionDate,
                Notes = transaction.Notes,
                CreatedAt = transaction.CreatedAt
            };
        }
    }
}