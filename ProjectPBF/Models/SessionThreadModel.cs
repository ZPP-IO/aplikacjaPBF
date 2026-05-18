using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class SessionThreadModel
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public SessionModel Session { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        public int CreatedByUserId { get; set; }
        public UserModel CreatedByUser { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SessionPostModel> Posts { get; set; } = new List<SessionPostModel>();
    }
}