using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trabajo_Final.models
{
    public enum TransactionType
    {
        BUY,
        SELL
    }

    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        public int CryptocurrencyId { get; set; }

        [Required]
        [Column(TypeName = "decimal(20,8)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal? PriceArs { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal? TotalAmountArs { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("CryptocurrencyId")]
        public virtual Cryptocurrency Cryptocurrency { get; set; } = null!;
    }
}
