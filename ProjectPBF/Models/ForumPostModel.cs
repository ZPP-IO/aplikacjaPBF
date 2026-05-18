using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectPBF.Models
{
    public class ForumPostModel
    {
        public int Id { get; set; }

        [Required]
        public int ThreadId { get; set; }

        [ForeignKey(nameof(ThreadId))]
        public virtual ForumThreadModel? Thread { get; set; }

        // Zawsze przechowujemy autora konta (UserId)
        public string? UserId { get; set; }

        // Opcjonalnie — jeœli post jest pisany w imieniu postaci
        public int? CharacterId { get; set; }

        // Snapshot nazw, by zachowaæ wyœwietlane imiê nawet po zmianie profilu
        [Required, MaxLength(200)]
        public string AuthorDisplayName { get; set; } = null!;

        [MaxLength(200)]
        public string? CharacterDisplayName { get; set; }

        [Required]
        public string Content { get; set; } = null!; // Markdown/BBCode/HTML — wybór frontendowy

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EditedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Cytowanie prost¹ relacj¹ do innego posta
        public int? QuotePostId { get; set; }

        [ForeignKey(nameof(QuotePostId))]
        public virtual ForumPostModel? QuotePost { get; set; }

        public virtual ICollection<ForumPostRevisionModel> Revisions { get; set; } = new List<ForumPostRevisionModel>();
    }
}