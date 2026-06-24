namespace ProjectPBF.ViewModels
{
    public class CalendarEventsIndexViewModel
    {
        public List<CalendarEventItemViewModel> Events { get; set; } = new();
        public string Scope { get; set; } = "all";
        public bool CanCreateEvents { get; set; }
    }
}
