namespace ProjectPBF.ViewModels
{
    public class HomeDashboardViewModel
    {
        public List<CalendarEventItemViewModel> UpcomingGlobalEvents { get; set; } = new();
        public List<CalendarEventItemViewModel> UpcomingPrivateEvents { get; set; } = new();
    }
}
