using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.ViewModels
{
    public class CampaignCreateViewModel
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Nazwa kampanii")]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        [Display(Name = "Opis kampanii")]
        public string Description { get; set; } = null!;

        [Range(0, 999)]
        [Display(Name = "Pula punktów do rozdania")]
        public int StartingPoints { get; set; } = 20;

        [Required]
        [Display(Name = "Statystyki")]
        public string StatisticsText { get; set; } = "Siła, Zręczność, Inteligencja";
    }
}
