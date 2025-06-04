using Trabajo_Final.DTOs.Transaction;

namespace Trabajo_Final.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionResponse?> CreateTransactionAsync(int userId, CreateTransactionRequest request);
        Task<List<TransactionResponse>> GetTransactionHistoryAsync(int userId, int? cryptoId = null, int page = 1, int pageSize = 50);
        Task<TransactionResponse?> GetTransactionByIdAsync(int userId, int transactionId);
        Task<bool> DeleteTransactionAsync(int userId, int transactionId);
        Task<TransactionResponse?> UpdateTransactionAsync(int userId, int transactionId, CreateTransactionRequest request);
    }
}
