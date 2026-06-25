using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Models
{
    // Zgloszenie posta, watku lub wiadomosci prywatnej do administracji.
    public class ReportModel
    {
        public int Id { get; set; }

        [Required]
        public ReportTargetType TargetType { get; set; }

        // Id zglaszanego obiektu (ForumPostModel.Id / ForumThreadModel.Id / PrivateMessageModel.Id)
        // - w zaleznosci od TargetType.
        [Required]
        public int TargetId { get; set; }

        // Krotki, czytelny opis tego co zostalo zglaszone (zapisywany w momencie
        // zgloszenia, zeby zostal widoczny dla admina nawet jesli orginal zostanie usuniety/zmieniony).
        [MaxLength(300)]
        public string? TargetSnapshot { get; set; }

        [Required]
        public int ReporterUserId { get; set; }

        [ForeignKey(nameof(ReporterUserId))]
        public virtual UserModel? Reporter { get; set; }

        [Required, MaxLength(1000)]
        public string Reason { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ReportStatus Status { get; set; } = ReportStatus.New;

        public int? ReviewedByUserId { get; set; }

        [ForeignKey(nameof(ReviewedByUserId))]
        public virtual UserModel? ReviewedByUser { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(1000)]
        public string? AdminNote { get; set; }
    }
}
