using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class MissionReviewModel
    {
        public int Id { get; set; }

        public int MissionProposalId { get; set; }
        public MissionProposalModel MissionProposal { get; set; } = null!;

        public int ReviewerId { get; set; }
        public UserModel Reviewer { get; set; } = null!;

        [Range(1, 10)]
        [Display(Name = "Ocena 1–10")]
        public int Score { get; set; } = 5;

        [Display(Name = "Werdykt")]
        public MissionReviewDecision Decision { get; set; } = MissionReviewDecision.Approve;

        [Required]
        [MaxLength(3000)]
        [Display(Name = "Komentarz")]
        public string Comment { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
