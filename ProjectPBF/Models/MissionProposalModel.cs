using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class MissionProposalModel
    {
        public int Id { get; set; }

        [Display(Name = "Kampania")]
        public int CampaignId { get; set; }
        public CampaignModel Campaign { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        [Display(Name = "Tytuł misji / fabuły")]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        [Display(Name = "Krótki opis")]
        public string Summary { get; set; } = null!;

        [Required]
        [MaxLength(8000)]
        [Display(Name = "Opis fabularny")]
        public string Description { get; set; } = null!;

        [MaxLength(3000)]
        [Display(Name = "Cel misji")]
        public string? Objective { get; set; }

        [MaxLength(3000)]
        [Display(Name = "Proponowane nagrody")]
        public string? SuggestedRewards { get; set; }

        [MaxLength(3000)]
        [Display(Name = "Ryzyko / konsekwencje")]
        public string? Risks { get; set; }

        public MissionProposalStatus Status { get; set; } = MissionProposalStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        public int SubmittedByUserId { get; set; }
        public UserModel SubmittedByUser { get; set; } = null!;

        public ICollection<MissionReviewModel> Reviews { get; set; } = new List<MissionReviewModel>();
    }
}
