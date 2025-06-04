using System.ComponentModel.DataAnnotations;
using Trabajo_Final.models;

namespace Trabajo_Final.DTOs.Transaction
{
    public class CreateTransactionRequest
    {
        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        public int CryptocurrencyId { get; set; }

        [Required]
        [Range(0.00000001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal? PriceArs { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be greater than or equal to 0")]
        public decimal? TotalAmountArs { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        public string? Notes { get; set; }
    }
}
