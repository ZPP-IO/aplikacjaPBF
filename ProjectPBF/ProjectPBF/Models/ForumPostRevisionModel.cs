using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectPBF.Models
{
    public class ForumPostRevisionModel
    {
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [ForeignKey(nameof(PostId))]
        public virtual ForumPostModel? Post { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        public DateTime EditedAt { get; set; } = DateTime.UtcNow;

        // Kto edytowa³ (konto)
        public string? EditedByUserId { get; set; }
    }
}