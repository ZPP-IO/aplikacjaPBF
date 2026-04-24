using Microsoft.AspNetCore.Identity;
using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class UserModel : IdentityUser<int>
    {
        [Required]
        [MaxLength(30)]
        public string Nick { get; set; } = null!;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        public AccountStatus AccountStatus { get; set; } = AccountStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastSeenAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public int? ApprovedByUserId { get; set; }

        public UserModel? ApprovedByUser { get; set; }

        public ICollection<CharacterModel> Characters { get; set; } = new List<CharacterModel>();

        public ICollection<CampaignModel> LedCampaigns { get; set; } = new List<CampaignModel>();
    }
}