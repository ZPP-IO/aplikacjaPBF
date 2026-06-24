using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class ConflictRuleModel
    {
        public int Id { get; set; }

        [Display(Name = "Kampania")]
        public int CampaignId { get; set; }
        public CampaignModel Campaign { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        [Display(Name = "Nazwa zasady / tabeli")]
        public string Title { get; set; } = null!;

        [Display(Name = "Typ konfliktu")]
        public ConflictRuleType Type { get; set; } = ConflictRuleType.Combat;

        [Required]
        [MaxLength(6000)]
        [Display(Name = "Opis zasady")]
        public string Description { get; set; } = null!;

        [MaxLength(1000)]
        [Display(Name = "Wzór / rzut")]
        public string? Formula { get; set; }

        [MaxLength(3000)]
        [Display(Name = "Przykład użycia")]
        public string? Example { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }
        public UserModel CreatedByUser { get; set; } = null!;

        public ICollection<ConflictRuleRowModel> Rows { get; set; } = new List<ConflictRuleRowModel>();
    }
}
