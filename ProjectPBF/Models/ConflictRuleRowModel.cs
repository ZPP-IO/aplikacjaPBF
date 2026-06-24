using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class ConflictRuleRowModel
    {
        public int Id { get; set; }

        public int ConflictRuleId { get; set; }
        public ConflictRuleModel ConflictRule { get; set; } = null!;

        [Display(Name = "Minimum")]
        public int? MinValue { get; set; }

        [Display(Name = "Maksimum")]
        public int? MaxValue { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Wynik")]
        public string Outcome { get; set; } = null!;

        [MaxLength(3000)]
        [Display(Name = "Efekt mechaniczny / fabularny")]
        public string? Effect { get; set; }

        public int Order { get; set; }
    }
}
