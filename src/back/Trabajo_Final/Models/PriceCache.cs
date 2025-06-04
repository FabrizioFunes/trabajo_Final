using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trabajo_Final.models
{
    public class PriceCache
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CryptocurrencyId { get; set; }

        [Required]
        public int ExchangeId { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal PriceArs { get; set; }

        public DateTime CachedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; }

        // Navigation properties
        [ForeignKey("CryptocurrencyId")]
        public virtual Cryptocurrency Cryptocurrency { get; set; } = null!;

        [ForeignKey("ExchangeId")]
        public virtual Exchange Exchange { get; set; } = null!;
    }
}
