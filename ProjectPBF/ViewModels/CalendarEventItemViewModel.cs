namespace ProjectPBF.ViewModels
{
    public class CalendarEventItemViewModel
    {
        public int Id { get; set; }
        public string Source { get; set; } = "Event";
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? CampaignTitle { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string? Location { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsImportant { get; set; }
        public bool IsCancelled { get; set; }
        public string Controller { get; set; } = "CalendarEvents";
        public string Action { get; set; } = "Details";

        public int DaysLeft
        {
            get
            {
                var today = DateTime.Today;
                return (StartAt.Date - today).Days;
            }
        }

        public string DateBadge
        {
            get
            {
                if (DaysLeft == 0) return "Dziś";
                if (DaysLeft == 1) return "Jutro";
                if (DaysLeft > 1 && DaysLeft <= 99) return $"{DaysLeft}d";
                return StartAt.ToString("dd.MM");
            }
        }

        public string TypeLabel
        {
            get
            {
                if (Source == "Session") return "Sesja";
                return IsGlobal ? "Globalne" : "Kampania";
            }
        }
    }
}
