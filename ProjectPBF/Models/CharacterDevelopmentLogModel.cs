using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class CharacterDevelopmentLogModel
    {
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public CharacterModel Character { get; set; } = null!;

        [Display(Name = "Zmiana EXP")]
        public int ExperienceChange { get; set; }

        [Display(Name = "Zmiana PH")]
        public int HistoryPointsChange { get; set; }

        [Display(Name = "Zmiana PK statystyk")]
        public int StatisticPointsChange { get; set; }

        [Required]
        [MaxLength(1000)]
        [Display(Name = "Powód")]
        public string Reason { get; set; } = null!;

        [MaxLength(120)]
        [Display(Name = "Źródło")]
        public string? Source { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedByUserId { get; set; }
        public UserModel? CreatedByUser { get; set; }
    }
}
