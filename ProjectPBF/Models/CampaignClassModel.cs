using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectPBF.Models
{
    public class CampaignClassModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nazwa klasy")]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        [Display(Name = "Opis klasy")]
        public string Description { get; set; } = null!;

        [MaxLength(120)]
        [Display(Name = "Główna statystyka")]
        public string? MainStatistic { get; set; }

        [Range(0, 9999)]
        [Display(Name = "Startowe HP")]
        public int StartingHealth { get; set; } = 10;

        [Range(0, 9999)]
        [Display(Name = "Startowa energia / mana")]
        public int StartingResource { get; set; } = 3;

        [Range(0, 9999)]
        [Display(Name = "Startowe złoto")]
        public int StartingGold { get; set; } = 0;

        [Display(Name = "Aktywna")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Campaign))]
        public int CampaignId { get; set; }
        public CampaignModel Campaign { get; set; } = null!;
    }
}
