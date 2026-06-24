using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models.Enums
{
    public enum CalendarEventVisibility
    {
        [Display(Name = "Globalne")]
        Global = 0,

        [Display(Name = "Prywatne / kampanijne")]
        Campaign = 1
    }
}
