using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trabajo_Final.models
{
    public class UserPreference
    {
        [Key]
        public int Id { get; set; }
         
        [Required]
        public int UserId { get; set; }

        public int? PreferredExchangeId { get; set; }

        [StringLength(10)]
        public string DefaultCurrency { get; set; } = "ARS";

        public bool EmailNotifications { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PreferredExchangeId")]
        public virtual Exchange? PreferredExchange { get; set; }
    }
}
