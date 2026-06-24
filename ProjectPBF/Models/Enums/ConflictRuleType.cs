using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models.Enums
{
    public enum ConflictRuleType
    {
        [Display(Name = "Walka")]
        Combat = 0,

        [Display(Name = "Wydarzenie fabularne")]
        StoryEvent = 1,

        [Display(Name = "Test społeczny")]
        Social = 2,

        [Display(Name = "Eksploracja")]
        Exploration = 3,

        [Display(Name = "Magia / technika")]
        Power = 4,

        [Display(Name = "Inne")]
        Other = 99
    }
}
