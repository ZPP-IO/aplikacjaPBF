using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class CampaignModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nazwa kampanii / sesji")]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        [Display(Name = "Opis")]
        public string Description { get; set; } = null!;

        [Range(0, 999)]
        [Display(Name = "Pula punktów do rozdania")]
        public int StartingPoints { get; set; } = 20;

        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int GameMasterId { get; set; }
        public UserModel GameMaster { get; set; } = null!;

        public ICollection<CampaignCharacterModel> CampaignCharacters { get; set; } = new List<CampaignCharacterModel>();
        public ICollection<CampaignMemberModel> Members { get; set; } = new List<CampaignMemberModel>();
        public ICollection<CampaignStatisticModel> Statistics { get; set; } = new List<CampaignStatisticModel>();
    }
}
