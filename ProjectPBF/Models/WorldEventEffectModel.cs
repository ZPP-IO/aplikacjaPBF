using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectPBF.Models
{
    public class WorldEventEffectModel
    {
        public int Id { get; set; }

        [Required]
        public int WorldEventId { get; set; }

        [ForeignKey(nameof(WorldEventId))]
        public virtual WorldEventModel? WorldEvent { get; set; }

        [Required]
        public int CharacterId { get; set; }

        [ForeignKey(nameof(CharacterId))]
        public virtual CharacterModel? Character { get; set; }

        public int ExperienceChange { get; set; } = 0;

        public int HistoryPointsChange { get; set; } = 0;

        public int StatisticPointsChange { get; set; } = 0;

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public int AppliedByUserId { get; set; }

        [ForeignKey(nameof(AppliedByUserId))]
        public virtual UserModel? AppliedByUser { get; set; }
    }
}