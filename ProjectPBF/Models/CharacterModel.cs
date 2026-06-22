using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectPBF.Models
{
    public class CharacterModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(60)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        public string Description { get; set; } = null!;

        // Zostają dla starych widoków tworzenia postaci.
        // Docelowe statystyki zależne od kampanii są w CharacterStatisticValueModel.
        [Range(0, 100)]
        public int Strength { get; set; }

        [Range(0, 100)]
        public int Agility { get; set; }

        [Range(0, 100)]
        public int Intelligence { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? AvatarUrl { get; set; }

        public CharacterStatus Status { get; set; } = CharacterStatus.Pending;

        [Range(0, 999999)]
        [Display(Name = "Doświadczenie")]
        public int Experience { get; set; } = 0;

        [Range(0, 999999)]
        [Display(Name = "Punkty Historii")]
        public int HistoryPoints { get; set; } = 0;

        [Range(1, 999)]
        [Display(Name = "Poziom")]
        public int Level { get; set; } = 1;

        [Range(0, 999999)]
        [Display(Name = "Wolne PK statystyk")]
        public int AvailableStatisticPoints { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public UserModel User { get; set; } = null!;

        [Display(Name = "Klasa postaci")]
        public int? CampaignClassId { get; set; }
        public CampaignClassModel? CampaignClass { get; set; }

        public ICollection<CampaignCharacterModel> CampaignCharacters { get; set; } = new List<CampaignCharacterModel>();
        public ICollection<CharacterStatisticValueModel> StatisticValues { get; set; } = new List<CharacterStatisticValueModel>();
        public ICollection<CharacterSkillModel> Skills { get; set; } = new List<CharacterSkillModel>();
        public ICollection<InventoryItemModel> InventoryItems { get; set; } = new List<InventoryItemModel>();
        public ICollection<CharacterDevelopmentLogModel> DevelopmentLogs { get; set; } = new List<CharacterDevelopmentLogModel>();
    }
}
