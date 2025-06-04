using System.ComponentModel.DataAnnotations;

namespace Trabajo_Final.models
{
    public class Exchange
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CriptoyaCode { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<PriceCache> PriceCaches { get; set; } = new List<PriceCache>();
        public virtual ICollection<UserPreference> UserPreferences { get; set; } = new List<UserPreference>();
    }
}
