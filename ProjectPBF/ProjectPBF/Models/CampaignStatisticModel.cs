using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class CampaignStatisticModel
    {
        public int Id { get; set; }

        public int CampaignId { get; set; }
        public CampaignModel Campaign { get; set; } = null!;

        [Required]
        [MaxLength(80)]
        [Display(Name = "Nazwa statystyki")]
        public string Name { get; set; } = null!;

        [Range(0, 999)]
        [Display(Name = "Wartość domyślna")]
        public int DefaultValue { get; set; } = 0;
    }
}
