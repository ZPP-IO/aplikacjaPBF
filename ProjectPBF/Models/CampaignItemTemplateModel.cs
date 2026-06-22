using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectPBF.Models
{
    public class CampaignItemTemplateModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        [Display(Name = "Nazwa przedmiotu")]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        [Display(Name = "Opis")]
        public string Description { get; set; } = null!;

        [Display(Name = "Typ")]
        public CampaignItemType ItemType { get; set; } = CampaignItemType.Other;

        [MaxLength(80)]
        [Display(Name = "Rzadkość")]
        public string? Rarity { get; set; }

        [MaxLength(4000)]
        [Display(Name = "Efekt mechaniczny")]
        public string? Effect { get; set; }

        [Range(0, 999999)]
        [Display(Name = "Cena / wartość")]
        public int Price { get; set; } = 0;

        [Display(Name = "Dostępny w kampanii")]
        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Campaign))]
        public int CampaignId { get; set; }
        public CampaignModel Campaign { get; set; } = null!;
    }
}
