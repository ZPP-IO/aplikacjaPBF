using ProjectPBF.Models.Enums;

namespace ProjectPBF.Models
{
    public class CampaignCharacterModel
    {
        public int Id { get; set; }

        public int CampaignId { get; set; }

        public CampaignModel Campaign { get; set; } = null!;

        public int CharacterId { get; set; }

        public CharacterModel Character { get; set; } = null!;

        public ParticipationStatus Status { get; set; } = ParticipationStatus.Pending;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}