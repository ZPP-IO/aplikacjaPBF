using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models.Enums
{
    public enum CampaignSkillTemplateKind
    {
        [Display(Name = "Umiejętność")]
        Skill = 0,

        [Display(Name = "Technika")]
        Technique = 1,

        [Display(Name = "Zaklęcie")]
        Spell = 2,

        [Display(Name = "Pasywka")]
        Passive = 3,

        [Display(Name = "Specjalna")]
        Special = 4
    }
}
