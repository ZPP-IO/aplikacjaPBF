using ProjectPBF.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectPBF.Models
{
    public class CharacterModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(60)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        public string Description { get; set; } = null!;

        [Range(0, 100)]
        public int Strength { get; set; }

        [Range(0, 100)]
        public int Agility { get; set; }

        [Range(0, 100)]
        public int Intelligence { get; set; }

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        public CharacterStatus Status { get; set; } = CharacterStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public UserModel User { get; set; } = null!;
    }
}