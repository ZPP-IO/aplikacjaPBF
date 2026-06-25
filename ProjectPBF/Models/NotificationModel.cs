using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Models
{
    // Powiadomienie generowane dla konkretnego uzytkownika.
    public class NotificationModel
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserModel? User { get; set; }

        public NotificationType Type { get; set; }

        [Required, MaxLength(300)]
        public string Message { get; set; } = null!;

        [Required, MaxLength(300)]
        public string LinkUrl { get; set; } = null!;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? RelatedThreadId { get; set; }

        public int? RelatedMessageId { get; set; }
    }
}