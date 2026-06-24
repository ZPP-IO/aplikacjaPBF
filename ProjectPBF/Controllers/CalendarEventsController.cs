using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;
using ProjectPBF.ViewModels;

namespace ProjectPBF.Controllers
{
    public class CalendarEventsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CalendarEventsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string scope = "all")
        {
            var model = new CalendarEventsIndexViewModel
            {
                Scope = string.IsNullOrWhiteSpace(scope) ? "all" : scope,
                CanCreateEvents = User.IsInRole("Administrator") || User.IsInRole("GameMaster")
            };

            var isAuthenticated = User.Identity?.IsAuthenticated == true;
            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Administrator");

            var calendarEventsQuery = _db.CalendarEvents
                .AsNoTracking()
                .Include(e => e.Campaign)
                .Where(e => !e.IsCancelled);

            if (!isAdmin)
            {
                if (isAuthenticated && currentUserId.HasValue)
                {
                    var userId = currentUserId.Value;
                    calendarEventsQuery = calendarEventsQuery.Where(e =>
                        e.Visibility == CalendarEventVisibility.Global
                        || (e.Visibility == CalendarEventVisibility.Campaign
                            && e.CampaignId != null
                            && (e.Campaign!.GameMasterId == userId
                                || e.Campaign.Members.Any(m => m.UserId == userId))));
                }
                else
                {
                    calendarEventsQuery = calendarEventsQuery.Where(e => e.Visibility == CalendarEventVisibility.Global);
                }
            }

            if (model.Scope == "global")
            {
                calendarEventsQuery = calendarEventsQuery.Where(e => e.Visibility == CalendarEventVisibility.Global);
            }
            else if (model.Scope == "private")
            {
                calendarEventsQuery = calendarEventsQuery.Where(e => e.Visibility == CalendarEventVisibility.Campaign);
            }

            var eventItems = await calendarEventsQuery
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
                    IsGlobal = e.Visibility == CalendarEventVisibility.Global,
                    IsPrivate = e.Visibility == CalendarEventVisibility.Campaign,
                    IsImportant = e.IsImportant,
                    IsCancelled = e.IsCancelled,
                    Controller = "CalendarEvents",
                    Action = "Details"
                })
                .ToListAsync();

            if (model.Scope != "global")
            {
                var sessionQuery = _db.SessionModels
                    .AsNoTracking()
                    .Include(s => s.Campaign)
                    .Where(s => s.StartAt.HasValue);

                if (!isAdmin)
                {
                    if (isAuthenticated && currentUserId.HasValue)
                    {
                        var userId = currentUserId.Value;
                        sessionQuery = sessionQuery.Where(s =>
                            !s.IsPrivate
                            || s.GameMasterId == userId
                            || s.Members.Any(m => m.UserId == userId && m.Status == SessionMembershipStatus.Accepted));
                    }
                    else
                    {
                        sessionQuery = sessionQuery.Where(s => !s.IsPrivate);
                    }
                }

                var sessionItems = await sessionQuery
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

                eventItems.AddRange(sessionItems);
            }

            model.Events = eventItems
                .OrderBy(e => e.StartAt < DateTime.Now)
                .ThenBy(e => e.StartAt)
                .ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var calendarEvent = await _db.CalendarEvents
                .Include(e => e.Campaign)
                .Include(e => e.CreatedByUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (calendarEvent == null) return NotFound();
            if (!await CanViewEvent(calendarEvent)) return Forbid();

            ViewBag.CanManage = await CanManageEvent(calendarEvent);
            return View(calendarEvent);
        }

        [Authorize(Roles = "Administrator,GameMaster")]
        public async Task<IActionResult> Create(int? campaignId = null)
        {
            var model = new CalendarEventCreateViewModel
            {
                CampaignId = campaignId,
                Visibility = campaignId.HasValue ? CalendarEventVisibility.Campaign : CalendarEventVisibility.Global,
                EventDate = DateTime.Now.AddDays(7)
            };

            await FillCampaigns(model.CampaignId);
            ViewBag.CanCreateGlobal = User.IsInRole("Administrator");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,GameMaster")]
        public async Task<IActionResult> Create(CalendarEventCreateViewModel model)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Challenge();

            var isAdmin = User.IsInRole("Administrator");

            if (model.EndDate.HasValue && model.EndDate.Value < model.EventDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "Data zakończenia nie może być wcześniejsza niż data rozpoczęcia.");
            }

            if (model.Visibility == CalendarEventVisibility.Global)
            {
                if (!isAdmin)
                {
                    ModelState.AddModelError(nameof(model.Visibility), "Tylko administrator może tworzyć wydarzenia globalne.");
                }

                model.CampaignId = null;
            }
            else
            {
                if (!model.CampaignId.HasValue)
                {
                    ModelState.AddModelError(nameof(model.CampaignId), "Wybierz kampanię dla wydarzenia prywatnego.");
                }
                else if (!await CanManageCampaign(model.CampaignId.Value, userId.Value, isAdmin))
                {
                    return Forbid();
                }
            }

            if (!ModelState.IsValid)
            {
                await FillCampaigns(model.CampaignId);
                ViewBag.CanCreateGlobal = isAdmin;
                return View(model);
            }

            var calendarEvent = new CalendarEventModel
            {
                Title = model.Title,
                Description = model.Description,
                EventDate = model.EventDate,
                EndDate = model.EndDate,
                Location = model.Location,
                Visibility = model.Visibility,
                CampaignId = model.CampaignId,
                IsImportant = model.IsImportant,
                CreatedByUserId = userId.Value,
                CreatedAt = DateTime.UtcNow
            };

            _db.CalendarEvents.Add(calendarEvent);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wydarzenie zostało dodane do kalendarza.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,GameMaster")]
        public async Task<IActionResult> Cancel(int id)
        {
            var calendarEvent = await _db.CalendarEvents.FirstOrDefaultAsync(e => e.Id == id);
            if (calendarEvent == null) return NotFound();
            if (!await CanManageEvent(calendarEvent)) return Forbid();

            calendarEvent.IsCancelled = true;
            _db.CalendarEvents.Update(calendarEvent);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wydarzenie zostało anulowane.";
            return RedirectToAction(nameof(Index));
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }

        private async Task FillCampaigns(int? selectedCampaignId = null)
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Administrator");

            IQueryable<CampaignModel> campaignsQuery = _db.CampaignModels.AsNoTracking();
            if (!isAdmin && userId.HasValue)
            {
                campaignsQuery = campaignsQuery.Where(c => c.GameMasterId == userId.Value);
            }

            var campaigns = await campaignsQuery.OrderBy(c => c.Title).ToListAsync();
            ViewBag.Campaigns = new SelectList(campaigns, "Id", "Title", selectedCampaignId);
        }

        private async Task<bool> CanManageCampaign(int campaignId, int userId, bool isAdmin)
        {
            if (isAdmin) return true;
            return await _db.CampaignModels.AnyAsync(c => c.Id == campaignId && c.GameMasterId == userId);
        }

        private async Task<bool> CanViewEvent(CalendarEventModel calendarEvent)
        {
            if (calendarEvent.Visibility == CalendarEventVisibility.Global) return true;
            if (!(User.Identity?.IsAuthenticated ?? false)) return false;
            if (User.IsInRole("Administrator")) return true;

            var userId = GetCurrentUserId();
            if (!userId.HasValue || !calendarEvent.CampaignId.HasValue) return false;

            return await _db.CampaignModels.AnyAsync(c => c.Id == calendarEvent.CampaignId.Value && c.GameMasterId == userId.Value)
                || await _db.CampaignMembers.AnyAsync(m => m.CampaignId == calendarEvent.CampaignId.Value && m.UserId == userId.Value);
        }

        private async Task<bool> CanManageEvent(CalendarEventModel calendarEvent)
        {
            if (!(User.Identity?.IsAuthenticated ?? false)) return false;
            if (User.IsInRole("Administrator")) return true;

            var userId = GetCurrentUserId();
            if (!userId.HasValue) return false;

            if (calendarEvent.Visibility == CalendarEventVisibility.Global) return false;
            if (!calendarEvent.CampaignId.HasValue) return false;

            return await _db.CampaignModels.AnyAsync(c => c.Id == calendarEvent.CampaignId.Value && c.GameMasterId == userId.Value);
        }
    }
}
