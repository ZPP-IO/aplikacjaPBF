using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class WorldEventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public WorldEventsController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Lista wydarzen - log historii swiata, z opcjonalnym filtrem po kampanii
        public async Task<IActionResult> Index(int? campaignId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var canSeeAll = await IsGlobalStaff(user);

            var query = _context.WorldEvents
                .Include(e => e.Campaign)
                .Include(e => e.CreatedByUser)
                .Include(e => e.Effects)
                .AsQueryable();

            if (campaignId.HasValue)
            {
                query = query.Where(e => e.CampaignId == campaignId.Value);
            }

            if (!canSeeAll)
            {
                var userId = user.Id;
                query = query.Where(e => e.Campaign!.GameMasterId == userId || e.Campaign!.Members.Any(m => m.UserId == userId));
            }

            var events = await query
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            ViewBag.CampaignId = campaignId;
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var worldEvent = await _context.WorldEvents
                .Include(e => e.Campaign).ThenInclude(c => c!.Members)
                .Include(e => e.CreatedByUser)
                .Include(e => e.Effects).ThenInclude(ef => ef.Character)
                .Include(e => e.Effects).ThenInclude(ef => ef.AppliedByUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (worldEvent == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanViewCampaign(worldEvent.Campaign, user)) return Forbid();

            ViewBag.CanManage = await CanManageCampaign(worldEvent.Campaign, user);

            // postacie z tej kampanii, ktore jeszcze nie otrzymaly efektu z tego wydarzenia
            var alreadyAffectedIds = worldEvent.Effects.Select(ef => ef.CharacterId).ToHashSet();
            var campaignCharacters = await _context.CampaignCharacterModels
                .Include(cc => cc.Character)
                .Where(cc => cc.CampaignId == worldEvent.CampaignId && cc.Status == ParticipationStatus.Accepted)
                .ToListAsync();

            ViewBag.EligibleCharacters = campaignCharacters
                .Where(cc => cc.Character != null && !alreadyAffectedIds.Contains(cc.Character.Id))
                .Select(cc => cc.Character)
                .ToList();

            return View(worldEvent);
        }

        public async Task<IActionResult> Create(int campaignId)
        {
            var campaign = await _context.CampaignModels
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanManageCampaign(campaign, user)) return Forbid();

            ViewBag.Campaign = campaign;
            return View(new WorldEventModel { CampaignId = campaignId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int campaignId, string title, WorldEventType type, string description, string? inGameDate, string? mechanicalImpactNote)
        {
            var campaign = await _context.CampaignModels
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanManageCampaign(campaign, user)) return Forbid();

            if (string.IsNullOrWhiteSpace(title)) ModelState.AddModelError(nameof(title), "Podaj tytuł wydarzenia.");
            if (string.IsNullOrWhiteSpace(description)) ModelState.AddModelError(nameof(description), "Podaj opis wydarzenia.");

            if (!ModelState.IsValid)
            {
                ViewBag.Campaign = campaign;
                return View(new WorldEventModel
                {
                    CampaignId = campaignId,
                    Title = title,
                    Type = type,
                    Description = description,
                    InGameDate = inGameDate,
                    MechanicalImpactNote = mechanicalImpactNote
                });
            }

            var worldEvent = new WorldEventModel
            {
                CampaignId = campaignId,
                Title = title.Trim(),
                Type = type,
                Description = description.Trim(),
                InGameDate = string.IsNullOrWhiteSpace(inGameDate) ? null : inGameDate.Trim(),
                MechanicalImpactNote = string.IsNullOrWhiteSpace(mechanicalImpactNote) ? null : mechanicalImpactNote.Trim(),
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.WorldEvents.Add(worldEvent);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = worldEvent.Id });
        }

        // Zastosowanie efektu mechanicznego wydarzenia na wybrane postacie - przyznaje EXP/PH/PK
        // przez wspolna logike z CharacterProgressController i zapisuje WorldEventEffectModel jako trwaly slad.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyEffect(int worldEventId, int[] characterIds, int expChange, int phChange, int statPointsChange)
        {
            var worldEvent = await _context.WorldEvents
                .Include(e => e.Campaign).ThenInclude(c => c!.Members)
                .FirstOrDefaultAsync(e => e.Id == worldEventId);

            if (worldEvent == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanManageCampaign(worldEvent.Campaign, user)) return Forbid();

            if (characterIds == null || characterIds.Length == 0)
            {
                TempData["ErrorMessage"] = "Wybierz przynajmniej jedną postać.";
                return RedirectToAction(nameof(Details), new { id = worldEventId });
            }

            var characters = await _context.CharacterModels
                .Include(c => c.CampaignCharacters).ThenInclude(cc => cc.Campaign)
                .Where(c => characterIds.Contains(c.Id))
                .ToListAsync();

            foreach (var character in characters)
            {
                await CharacterProgressController.ApplyAwardAsync(
                    _context,
                    character,
                    expChange,
                    phChange,
                    statPointsChange,
                    $"Wydarzenie światowe: {worldEvent.Title}",
                    $"WorldEvent:{worldEvent.Id}",
                    user.Id
                );

                _context.WorldEventEffects.Add(new WorldEventEffectModel
                {
                    WorldEventId = worldEvent.Id,
                    CharacterId = character.Id,
                    ExperienceChange = expChange,
                    HistoryPointsChange = phChange,
                    StatisticPointsChange = statPointsChange,
                    AppliedByUserId = user.Id,
                    AppliedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = worldEventId });
        }

        private async Task<bool> IsGlobalStaff(UserModel user)
        {
            return await _userManager.IsInRoleAsync(user, "Administrator") || await _userManager.IsInRoleAsync(user, "GameMaster");
        }

        private async Task<bool> CanViewCampaign(CampaignModel? campaign, UserModel user)
        {
            if (campaign == null) return false;
            if (await IsGlobalStaff(user)) return true;
            return campaign.GameMasterId == user.Id || campaign.Members.Any(m => m.UserId == user.Id);
        }

        private async Task<bool> CanManageCampaign(CampaignModel? campaign, UserModel user)
        {
            if (campaign == null) return false;
            if (await IsGlobalStaff(user)) return true;
            return campaign.GameMasterId == user.Id || campaign.Members.Any(m => m.UserId == user.Id && m.IsAdmin);
        }
    }
}