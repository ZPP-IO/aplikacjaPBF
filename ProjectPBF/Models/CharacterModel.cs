using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ProjectPBF.Models
{
    public class CharacterModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Imię Postaci")]
        public string Name { get; set; }

        [Display(Name = "Opis/Biografia")]
        public string Description { get; set; }

        public int Strength { get; set; } = 10;
        public int Agility { get; set; } = 10;
        public int Intelligence { get; set; } = 10;

        public string? AvatarUrl { get; set; }

        [Required]
        public bool IsAccepted { get; set; } = false;
        public int UserId { get; set; }
        public virtual UserModel User { get; set; }
    }
}