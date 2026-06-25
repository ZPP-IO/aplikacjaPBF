using ProjectPBF.Models.Enums;

namespace ProjectPBF.ViewModels
{
    /// <summary>
    /// Obliczony status aktywności użytkownika łączący ręczny wybór z automatycznym offline.
    /// </summary>
    public enum ComputedOnlineStatus
    {
        Online,    // LastSeenAt < 15 min temu + status = Available
        Busy,      // LastSeenAt < 15 min temu + status = Busy
        Away,      // LastSeenAt między 15 a 60 min temu
        Offline    // LastSeenAt > 60 min temu lub brak danych
    }

    public class CommunityUserCardViewModel
    {
        public int Id { get; set; }
        public string Nick { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CharacterCount { get; set; }
        public int CampaignCount { get; set; }
        public PresenceStatus ManualStatus { get; set; }
        public ComputedOnlineStatus OnlineStatus { get; set; }
        public DateTime? LastSeenAt { get; set; }
    }

    public class CommunityIndexViewModel
    {
        public List<CommunityUserCardViewModel> Users { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }
        public int TotalCount { get; set; }
        public int OnlineCount { get; set; }

        // Własny status zalogowanego użytkownika do widgetu zmiany statusu
        public PresenceStatus? CurrentUserStatus { get; set; }
    }
}
