using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;
using ProjectPBF.ViewModels;

namespace ProjectPBF.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var model = new HomeDashboardViewModel();

            model.UpcomingGlobalEvents = await _db.CalendarEvents
                .AsNoTracking()
                .Where(e => !e.IsCancelled
                    && e.Visibility == CalendarEventVisibility.Global
                    && e.EventDate >= now)
                .OrderBy(e => e.EventDate)
                .Take(5)
                .Select(e => new CalendarEventItemViewModel
                {
                    Id = e.Id,
                    Source = "Event",
                    Title = e.Title,
                    Description = e.Description,
                    StartAt = e.EventDate,
                    EndAt = e.EndDate,
                    Location = e.Location,
                    IsGlobal = true,
                    IsImportant = e.IsImportant,
                    IsCancelled = e.IsCancelled,
                    Controller = "CalendarEvents",
                    Action = "Details"
                })
                .ToListAsync();

            if (User.Identity?.IsAuthenticated == true && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId))
            {
                var campaignEvents = await _db.CalendarEvents
                    .AsNoTracking()
                    .Include(e => e.Campaign)
                    .Where(e => !e.IsCancelled
                        && e.Visibility == CalendarEventVisibility.Campaign
                        && e.EventDate >= now
                        && e.CampaignId != null
                        && (e.Campaign!.GameMasterId == currentUserId
                            || e.Campaign.Members.Any(m => m.UserId == currentUserId)))
                    .OrderBy(e => e.EventDate)
                    .Take(5)
                    .Select(e => new CalendarEventItemViewModel
                    {
                        Id = e.Id,
                        Source = "Event",
                        Title = e.Title,
                        Description = e.Description,
                        CampaignTitle = e.Campaign != null ? e.Campaign.Title : null,
                        StartAt = e.EventDate,
                        EndAt = e.EndDate,
                        Location = e.Location,
                        IsGlobal = false,
                        IsPrivate = true,
                        IsImportant = e.IsImportant,
                        IsCancelled = e.IsCancelled,
                        Controller = "CalendarEvents",
                        Action = "Details"
                    })
                    .ToListAsync();

                var sessionEvents = await _db.SessionModels
                    .AsNoTracking()
                    .Include(s => s.Campaign)
                    .Where(s => s.StartAt.HasValue
                        && s.StartAt.Value >= now
                        && (s.GameMasterId == currentUserId
                            || s.Members.Any(m => m.UserId == currentUserId && m.Status == SessionMembershipStatus.Accepted)))
                    .OrderBy(s => s.StartAt)
                    .Take(5)
                    .Select(s => new CalendarEventItemViewModel
                    {
                        Id = s.Id,
                        Source = "Session",
                        Title = s.Title,
                        Description = s.Description,
                        CampaignTitle = s.Campaign != null ? s.Campaign.Title : null,
                        StartAt = s.StartAt!.Value,
                        EndAt = s.StartAt!.Value.AddMinutes(s.DurationMinutes),
                        IsGlobal = false,
                        IsPrivate = s.IsPrivate,
                        Controller = "Sessions",
                        Action = "Details"
                    })
                    .ToListAsync();

                model.UpcomingPrivateEvents = campaignEvents
                    .Concat(sessionEvents)
                    .OrderBy(e => e.StartAt)
                    .Take(6)
                    .ToList();
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult AdminPanel()
        {
            return RedirectToAction("Index", "AdminPanel");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
