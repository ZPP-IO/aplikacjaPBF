using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models.Enums
{
    public enum CampaignItemType
    {
        [Display(Name = "Broń")]
        Weapon = 0,

        [Display(Name = "Pancerz")]
        Armor = 1,

        [Display(Name = "Mikstura / zużywalny")]
        Consumable = 2,

        [Display(Name = "Narzędzie")]
        Tool = 3,

        [Display(Name = "Artefakt")]
        Artifact = 4,

        [Display(Name = "Inne")]
        Other = 99
    }
}
