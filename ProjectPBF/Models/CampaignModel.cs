using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class CampaignModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        public string Description { get; set; } = null!;

        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int GameMasterId { get; set; }

        public UserModel GameMaster { get; set; } = null!;

        public ICollection<CampaignCharacterModel> CampaignCharacters { get; set; } = new List<CampaignCharacterModel>();
    }
}