using Trabajo_Final.models;

namespace Trabajo_Final.DTOs.Transaction
{
    public class TransactionResponse
    {
        public int Id { get; set; }
        public TransactionType TransactionType { get; set; }
        public string CryptocurrencySymbol { get; set; } = string.Empty;
        public string CryptocurrencyName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal? PriceArs { get; set; }
        public decimal? TotalAmountArs { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
