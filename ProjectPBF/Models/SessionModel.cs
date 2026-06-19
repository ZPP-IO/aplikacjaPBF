using System.ComponentModel.DataAnnotations;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Models
{
    public class SessionModel
    {
        public int Id { get; set; }

        public int CampaignId { get; set; }
        public CampaignModel? Campaign { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = null!;

        [MaxLength(4000)]
        public string? Description { get; set; }

        public int GameMasterId { get; set; }
        public UserModel? GameMaster { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartAt { get; set; }

        // Mo¿na u¿yæ CampaignStatus lub w razie potrzeby dodaæ SessionStatus enum.
        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

        // Nowe: czas trwania w minutach i prywatnoœæ sesji
        [Range(0, 7 * 24 * 60)]
        [Display(Name = "Czas trwania (minuty)")]
        public int DurationMinutes { get; set; } = 120;

        [Display(Name = "Sesja prywatna")]
        public bool IsPrivate { get; set; } = true;

        // Cz³onkowie i forum sesji
        public ICollection<SessionMemberModel> Members { get; set; } = new List<SessionMemberModel>();
        public ICollection<SessionThreadModel> Threads { get; set; } = new List<SessionThreadModel>();
    }
}