using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class SessionPostModel
    {
        public int Id { get; set; }

        public int ThreadId { get; set; }
        public SessionThreadModel Thread { get; set; } = null!;

        public int UserId { get; set; }
        public UserModel User { get; set; } = null!;

        [Required]
        [MaxLength(8000)]
        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; set; }
    }
}