namespace ProjectPBF.Models
{
    public class CampaignMemberModel
    {
        public int Id { get; set; }

        public int CampaignId { get; set; }
        public CampaignModel Campaign { get; set; } = null!;

        public int UserId { get; set; }
        public UserModel User { get; set; } = null!;

        public bool IsAdmin { get; set; } = false;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
