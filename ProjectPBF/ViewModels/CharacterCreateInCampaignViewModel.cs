using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.ViewModels
{
    public class CharacterCreateInCampaignViewModel
    {
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = string.Empty;
        public int StartingPoints { get; set; }

        [Display(Name = "Klasa postaci")]
        public int? CampaignClassId { get; set; }

        public List<SelectListItem> ClassOptions { get; set; } = new();

        [Required(ErrorMessage = "Podaj nazwę postaci.")]
        [MaxLength(60, ErrorMessage = "Nazwa postaci może mieć maksymalnie 60 znaków.")]
        [Display(Name = "Nazwa postaci")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj opis postaci.")]
        [MaxLength(4000, ErrorMessage = "Opis może mieć maksymalnie 4000 znaków.")]
        [Display(Name = "Opis")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(2048, ErrorMessage = "Link do avatara może mieć maksymalnie 2048 znaków. Nie wklejaj obrazka base64.")]
        [Display(Name = "Link do avatara")]
        public string? AvatarUrl { get; set; }

        public List<CharacterStatisticInputViewModel> Statistics { get; set; } = new();
    }
}
