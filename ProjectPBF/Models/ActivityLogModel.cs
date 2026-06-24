using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class ActivityLogModel
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public UserModel? User { get; set; }

        public int? CampaignId { get; set; }
        public CampaignModel? Campaign { get; set; }

        public int? CharacterId { get; set; }
        public CharacterModel? Character { get; set; }

        public ActivityActionType ActionType { get; set; } = ActivityActionType.Created;

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = null!;

        public int? EntityId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}