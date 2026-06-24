using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models.Enums
{
    public enum MissionProposalStatus
    {
        [Display(Name = "Oczekuje na ocenę")]
        Pending = 0,

        [Display(Name = "W trakcie oceniania")]
        InReview = 1,

        [Display(Name = "Zaakceptowana")]
        Accepted = 2,

        [Display(Name = "Do poprawy")]
        NeedsChanges = 3,

        [Display(Name = "Odrzucona")]
        Rejected = 4,

        [Display(Name = "Zakończona")]
        Completed = 5
    }
}
