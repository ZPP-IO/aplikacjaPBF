using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class CharacterStatisticValueModel
    {
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public CharacterModel Character { get; set; } = null!;

        public int CampaignStatisticId { get; set; }
        public CampaignStatisticModel Statistic { get; set; } = null!;

        [Range(0, 999)]
        public int Value { get; set; }
    }
}
