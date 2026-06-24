using System.ComponentModel.DataAnnotations;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.ViewModels
{
    public class CalendarEventCreateViewModel
    {
        [Required]
        [MaxLength(150)]
        [Display(Name = "Tytuł wydarzenia")]
        public string Title { get; set; } = null!;

        [MaxLength(4000)]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Data i godzina")]
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "Data zakończenia")]
        public DateTime? EndDate { get; set; }

        [MaxLength(150)]
        [Display(Name = "Miejsce / kanał")]
        public string? Location { get; set; }

        [Display(Name = "Widoczność")]
        public CalendarEventVisibility Visibility { get; set; } = CalendarEventVisibility.Campaign;

        [Display(Name = "Kampania")]
        public int? CampaignId { get; set; }

        [Display(Name = "Ważne wydarzenie")]
        public bool IsImportant { get; set; }
    }
}
