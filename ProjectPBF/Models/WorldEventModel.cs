using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Models
{
    // Wydarzenie swiatowe / historyczne ogloszone przez MG w ramach kampanii.
    public class WorldEventModel
    {
        public int Id { get; set; }

        [Required]
        public int CampaignId { get; set; }

        [ForeignKey(nameof(CampaignId))]
        public virtual CampaignModel? Campaign { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = null!;

        public WorldEventType Type { get; set; } = WorldEventType.Other;

        [Required, MaxLength(6000)]
        public string Description { get; set; } = null!;

        [MaxLength(100)]
        public string? InGameDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual UserModel? CreatedByUser { get; set; }

        [MaxLength(3000)]
        public string? MechanicalImpactNote { get; set; }

        public virtual ICollection<WorldEventEffectModel> Effects { get; set; } = new List<WorldEventEffectModel>();
    }
}