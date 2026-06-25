using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class CharacterProgressController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public CharacterProgressController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Award(int characterId)
        {
            var character = await _context.CharacterModels
                .Include(c => c.User)
                .Include(c => c.CampaignCharacters)
                    .ThenInclude(cc => cc.Campaign)
                        .ThenInclude(campaign => campaign.Members)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null) return NotFound();
            if (!await CanManageCharacterProgress(character)) return Forbid();

            SetAwardViewBagInstance(character);
            return View(character);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Award(int characterId, int expChange, int phChange, int statPointsChange, string reason, string? source)
        {
            var character = await _context.CharacterModels
                .Include(c => c.CampaignCharacters)
                    .ThenInclude(cc => cc.Campaign)
                        .ThenInclude(campaign => campaign.Members)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null) return NotFound();
            if (!await CanManageCharacterProgress(character)) return Forbid();

            if (string.IsNullOrWhiteSpace(reason))
            {
                ModelState.AddModelError(nameof(reason), "Podaj powód przyznania punktów.");
                SetAwardViewBagInstance(character);
                return View(character);
            }

            var userIdText = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdText)) return Challenge();

            await ApplyAwardAsync(_context, character, expChange, phChange, statPointsChange, reason, source, int.Parse(userIdText));

            return RedirectToAction("Details", "CharacterModels", new { id = character.Id });
        }

        // Wspolna logika przyznawania EXP/PH/PK statystyk postaci, wraz z wpisem do logu rozwoju.
        // Uzywana zarowno z formularza recznego przyznania (Award), jak i z efektow wydarzen swiatowych.
        public static async Task ApplyAwardAsync(
            ApplicationDbContext context,
            CharacterModel character,
            int expChange,
            int phChange,
            int statPointsChange,
            string reason,
            string? source,
            int byUserId)
        {
            var oldLevel = character.Level;
            character.Experience = Math.Max(0, character.Experience + expChange);
            character.HistoryPoints = Math.Max(0, character.HistoryPoints + phChange);
            character.Level = CalculateLevel(character.Experience);

            var campaign = character.CampaignCharacters.FirstOrDefault()?.Campaign;
            var levelUps = Math.Max(0, character.Level - oldLevel);
            var automaticPkFromLevel = levelUps * (campaign?.StatisticPointsPerLevel ?? 0);
            var totalStatPointsChange = statPointsChange + automaticPkFromLevel;

            character.AvailableStatisticPoints = Math.Max(0, character.AvailableStatisticPoints + totalStatPointsChange);
            character.UpdatedAt = DateTime.UtcNow;

            var finalReason = reason;
            if (automaticPkFromLevel > 0)
            {
                finalReason += $" | Awans o {levelUps} poziom(y): +{automaticPkFromLevel} PK statystyk.";
            }

            context.CharacterDevelopmentLogs.Add(new CharacterDevelopmentLogModel
            {
                CharacterId = character.Id,
                ExperienceChange = expChange,
                HistoryPointsChange = phChange,
                StatisticPointsChange = totalStatPointsChange,
                Reason = finalReason,
                Source = source,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = byUserId
            });

            await context.SaveChangesAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SpendStatPoint(int characterId, int statisticValueId, int amount = 1)
        {
            var userIdText = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdText)) return Challenge();

            var userId = int.Parse(userIdText);
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var character = await _context.CharacterModels
                .Include(c => c.StatisticValues).ThenInclude(sv => sv.Statistic)
                .Include(c => c.CampaignCharacters)
                    .ThenInclude(cc => cc.Campaign)
                        .ThenInclude(campaign => campaign.Members)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null) return NotFound();

            var isOwner = character.UserId == userId;
            var isStaff = await _userManager.IsInRoleAsync(currentUser, "Administrator") ||
                          await _userManager.IsInRoleAsync(currentUser, "GameMaster");

            var isCampaignOwnerOrAdmin = character.CampaignCharacters.Any(cc =>
                cc.Campaign != null &&
                (cc.Campaign.GameMasterId == userId || cc.Campaign.Members.Any(m => m.UserId == userId && m.IsAdmin)));

            if (!isOwner && !isStaff && !isCampaignOwnerOrAdmin)
            {
                return Forbid();
            }

            var stat = character.StatisticValues.FirstOrDefault(s => s.Id == statisticValueId);
            if (stat == null) return NotFound();

            var pointsToSpend = Math.Max(1, amount);
            pointsToSpend = Math.Min(pointsToSpend, character.AvailableStatisticPoints);

            if (pointsToSpend <= 0)
            {
                TempData["Error"] = "Brak wolnych PK do rozdania.";
                return RedirectToAction("Details", "CharacterModels", new { id = character.Id });
            }

            stat.Value += pointsToSpend;
            character.AvailableStatisticPoints -= pointsToSpend;
            character.UpdatedAt = DateTime.UtcNow;

            SyncOldStatColumns(character, stat.Statistic.Name, stat.Value);

            _context.CharacterDevelopmentLogs.Add(new CharacterDevelopmentLogModel
            {
                CharacterId = character.Id,
                ExperienceChange = 0,
                HistoryPointsChange = 0,
                StatisticPointsChange = -pointsToSpend,
                Reason = $"Rozdano {pointsToSpend} PK w statystykę: {stat.Statistic.Name}.",
                Source = "rozwój postaci",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "CharacterModels", new { id = character.Id });
        }

        private async Task<bool> CanManageCharacterProgress(CharacterModel character)
        {
            var userIdText = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdText)) return false;

            var userId = int.Parse(userIdText);
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return false;

            var isStaff = await _userManager.IsInRoleAsync(currentUser, "Administrator") ||
                          await _userManager.IsInRoleAsync(currentUser, "GameMaster");

            var isCampaignOwnerOrAdmin = character.CampaignCharacters.Any(cc =>
                cc.Campaign != null &&
                (cc.Campaign.GameMasterId == userId || cc.Campaign.Members.Any(m => m.UserId == userId && m.IsAdmin)));

            return isStaff || isCampaignOwnerOrAdmin;
        }

        private void SetAwardViewBagInstance(CharacterModel character)
        {
            var campaign = character.CampaignCharacters.FirstOrDefault()?.Campaign;
            ViewBag.CampaignTitle = campaign?.Title ?? "Brak kampanii";
            ViewBag.StatisticPointsPerLevel = campaign?.StatisticPointsPerLevel ?? 0;
        }

        private static int CalculateLevel(int experience)
        {
            return Math.Max(1, (experience / 100) + 1);
        }

        private static void SyncOldStatColumns(CharacterModel character, string statName, int value)
        {
            if (statName.Equals("Siła", StringComparison.OrdinalIgnoreCase) || statName.Equals("Sila", StringComparison.OrdinalIgnoreCase))
            {
                character.Strength = value;
            }
            else if (statName.Equals("Zręczność", StringComparison.OrdinalIgnoreCase) || statName.Equals("Zrecznosc", StringComparison.OrdinalIgnoreCase))
            {
                character.Agility = value;
            }
            else if (statName.Equals("Inteligencja", StringComparison.OrdinalIgnoreCase))
            {
                character.Intelligence = value;
            }
        }
    }
}