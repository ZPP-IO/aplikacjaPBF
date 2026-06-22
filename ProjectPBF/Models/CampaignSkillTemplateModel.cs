using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectPBF.Models
{
    public class CampaignSkillTemplateModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        [Display(Name = "Nazwa umiejętności / techniki")]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        [Display(Name = "Opis")]
        public string Description { get; set; } = null!;

        [Display(Name = "Typ")]
        public CampaignSkillTemplateKind Kind { get; set; } = CampaignSkillTemplateKind.Skill;

        [MaxLength(2000)]
        [Display(Name = "Wymagania")]
        public string? Requirements { get; set; }

        [Range(0, 999)]
        [Display(Name = "Koszt")]
        public int Cost { get; set; } = 0;

        [Range(0, 999)]
        [Display(Name = "Cooldown / ograniczenie")]
        public int Cooldown { get; set; } = 0;

        [MaxLength(4000)]
        [Display(Name = "Efekt mechaniczny")]
        public string? Effect { get; set; }

        [Display(Name = "Dostępna w kampanii")]
        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Campaign))]
        public int CampaignId { get; set; }
        public CampaignModel Campaign { get; set; } = null!;
    }
}
