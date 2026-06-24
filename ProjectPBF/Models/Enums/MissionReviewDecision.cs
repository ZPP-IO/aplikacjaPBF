using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models.Enums
{
    public enum MissionReviewDecision
    {
        [Display(Name = "Popieram")]
        Approve = 0,

        [Display(Name = "Do poprawy")]
        NeedsChanges = 1,

        [Display(Name = "Odrzucam")]
        Reject = 2
    }
}
