using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class CharacterSkillModel
    {
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public CharacterModel Character { get; set; } = null!;

        [Display(Name = "Wzorzec z biblioteki kampanii")]
        public int? CampaignSkillTemplateId { get; set; }
        public CampaignSkillTemplateModel? CampaignSkillTemplate { get; set; }

        [Required]
        [MaxLength(120)]
        [Display(Name = "Nazwa umiejętności / techniki")]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        [Display(Name = "Opis fabularny")]
        public string Description { get; set; } = null!;

        [Display(Name = "Typ")]
        public SkillKind Kind { get; set; } = SkillKind.Skill;

        [MaxLength(2000)]
        [Display(Name = "Wymagania")]
        public string? Requirements { get; set; }

        [MaxLength(4000)]
        [Display(Name = "Efekt mechaniczny")]
        public string? Effect { get; set; }

        [Range(0, 999)]
        [Display(Name = "Koszt użycia / aktywacji")]
        public int Cost { get; set; } = 0;

        [Range(0, 999)]
        [Display(Name = "Cooldown / limit użycia")]
        public int Cooldown { get; set; } = 0;

        [Range(0, 999)]
        [Display(Name = "Proponowany koszt PH")]
        public int RequestedHistoryPointCost { get; set; } = 0;

        [Display(Name = "Status")]
        public SkillStatus Status { get; set; } = SkillStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        public int? ReviewedByUserId { get; set; }
        public UserModel? ReviewedByUser { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Komentarz MG")]
        public string? ReviewComment { get; set; }
    }
}
