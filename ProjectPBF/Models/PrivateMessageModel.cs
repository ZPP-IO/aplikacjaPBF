using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectPBF.Models
{
    // Pojedyncza wiadomosc prywatna wyslana od jednego uzytkownika do drugiego.
    public class PrivateMessageModel
    {
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual UserModel? Sender { get; set; }

        [Required]
        public int RecipientId { get; set; }

        [ForeignKey(nameof(RecipientId))]
        public virtual UserModel? Recipient { get; set; }

        [Required, MaxLength(4000)]
        public string Content { get; set; } = null!;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }
    }
}