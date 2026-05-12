using System.ComponentModel.DataAnnotations;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Models
{
    public class SessionMemberModel
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public SessionModel Session { get; set; } = null!;

        public int UserId { get; set; }
        public UserModel User { get; set; } = null!;

        public bool IsAdmin { get; set; } = false;

        // Status proœby / cz³onkostwa
        public SessionMembershipStatus Status { get; set; } = SessionMembershipStatus.Pending;

        // Data do³¹czenia ustawiana przy akceptacji
        public DateTime? JoinedAt { get; set; }
    }
}