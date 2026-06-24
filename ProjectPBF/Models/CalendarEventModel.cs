using System.ComponentModel.DataAnnotations;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Models
{
    public class CalendarEventModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Display(Name = "Tytuł wydarzenia")]
        public string Title { get; set; } = null!;

        [MaxLength(4000)]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Data i godzina")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Data zakończenia")]
        public DateTime? EndDate { get; set; }

        [MaxLength(150)]
        [Display(Name = "Miejsce / kanał")]
        public string? Location { get; set; }

        [Display(Name = "Widoczność")]
        public CalendarEventVisibility Visibility { get; set; } = CalendarEventVisibility.Campaign;

        [Display(Name = "Ważne wydarzenie")]
        public bool IsImportant { get; set; }

        public bool IsCancelled { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }
        public UserModel CreatedByUser { get; set; } = null!;

        public int? CampaignId { get; set; }
        public CampaignModel? Campaign { get; set; }
    }
}
