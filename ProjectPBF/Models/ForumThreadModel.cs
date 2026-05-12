using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Models
{
    public class ForumThreadModel
    {
        public int Id { get; set; }

        [Required]
        public int ForumId { get; set; }

        [ForeignKey(nameof(ForumId))]
        public virtual ForumModel? Forum { get; set; }

        [Required, MaxLength(300)]
        public string Title { get; set; } = null!;

        // Autor w sensie konta
        public string? CreatedByUserId { get; set; }

        // Opcjonalnie: post utworzony w imieniu postaci
        public int? CreatedByCharacterId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsLocked { get; set; } = false;

        public bool IsPinned { get; set; } = false;

        public ThreadStatus Status { get; set; } = ThreadStatus.Open;

        public virtual ICollection<ForumPostModel> Posts { get; set; } = new List<ForumPostModel>();
    }
}