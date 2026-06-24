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
    public class ConflictRulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public ConflictRulesController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? campaignId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var canSeeAll = await IsGlobalStaff(user);

            var query = _context.ConflictRules
                .Include(r => r.Campaign)
                .Include(r => r.CreatedByUser)
                .Include(r => r.Rows)
                .AsQueryable();

            if (campaignId.HasValue)
            {
                query = query.Where(r => r.CampaignId == campaignId.Value);
            }

            if (!canSeeAll)
            {
                var userId = user.Id;
                query = query.Where(r => r.Campaign.GameMasterId == userId || r.Campaign.Members.Any(m => m.UserId == userId));
            }

            var rules = await query
                .OrderBy(r => r.Campaign.Title)
                .ThenBy(r => r.Type)
                .ThenBy(r => r.Title)
                .ToListAsync();

            ViewBag.CampaignId = campaignId;
            return View(rules);
        }

        public async Task<IActionResult> Details(int id)
        {
            var rule = await _context.ConflictRules
                .Include(r => r.Campaign).ThenInclude(c => c.Members)
                .Include(r => r.CreatedByUser)
                .Include(r => r.Rows)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rule == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!await CanViewCampaign(rule.Campaign, user)) return Forbid();

            ViewBag.CanManage = await CanManageCampaign(rule.Campaign, user);
            rule.Rows = rule.Rows.OrderBy(r => r.Order).ThenBy(r => r.MinValue).ToList();
            return View(rule);
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
            return View(new ConflictRuleModel { CampaignId = campaignId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int campaignId, string title, ConflictRuleType type, string description, string? formula, string? example)
        {
            var campaign = await _context.CampaignModels
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanManageCampaign(campaign, user)) return Forbid();

            if (string.IsNullOrWhiteSpace(title)) ModelState.AddModelError(nameof(title), "Podaj nazwę zasady.");
            if (string.IsNullOrWhiteSpace(description)) ModelState.AddModelError(nameof(description), "Podaj opis zasady.");

            if (!ModelState.IsValid)
            {
                ViewBag.Campaign = campaign;
                return View(new ConflictRuleModel
                {
                    CampaignId = campaignId,
                    Title = title,
                    Type = type,
                    Description = description,
                    Formula = formula,
                    Example = example
                });
            }

            var rule = new ConflictRuleModel
            {
                CampaignId = campaignId,
                Title = title.Trim(),
                Type = type,
                Description = description.Trim(),
                Formula = string.IsNullOrWhiteSpace(formula) ? null : formula.Trim(),
                Example = string.IsNullOrWhiteSpace(example) ? null : example.Trim(),
                CreatedByUserId = user.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.ConflictRules.Add(rule);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = rule.Id });
        }

        public async Task<IActionResult> AddRow(int id)
        {
            var rule = await _context.ConflictRules
                .Include(r => r.Campaign).ThenInclude(c => c.Members)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rule == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanManageCampaign(rule.Campaign, user)) return Forbid();

            ViewBag.Rule = rule;
            return View(new ConflictRuleRowModel { ConflictRuleId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRow(int conflictRuleId, int? minValue, int? maxValue, string outcome, string? effect, int order = 0)
        {
            var rule = await _context.ConflictRules
                .Include(r => r.Campaign).ThenInclude(c => c.Members)
                .FirstOrDefaultAsync(r => r.Id == conflictRuleId);

            if (rule == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanManageCampaign(rule.Campaign, user)) return Forbid();

            if (string.IsNullOrWhiteSpace(outcome)) ModelState.AddModelError(nameof(outcome), "Podaj wynik tabeli.");
            if (minValue.HasValue && maxValue.HasValue && minValue.Value > maxValue.Value)
            {
                ModelState.AddModelError(nameof(minValue), "Minimum nie może być większe od maksimum.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Rule = rule;
                return View(new ConflictRuleRowModel
                {
                    ConflictRuleId = conflictRuleId,
                    MinValue = minValue,
                    MaxValue = maxValue,
                    Outcome = outcome,
                    Effect = effect,
                    Order = order
                });
            }

            _context.ConflictRuleRows.Add(new ConflictRuleRowModel
            {
                ConflictRuleId = conflictRuleId,
                MinValue = minValue,
                MaxValue = maxValue,
                Outcome = outcome.Trim(),
                Effect = string.IsNullOrWhiteSpace(effect) ? null : effect.Trim(),
                Order = order
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = conflictRuleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var rule = await _context.ConflictRules
                .Include(r => r.Campaign).ThenInclude(c => c.Members)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rule == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanManageCampaign(rule.Campaign, user)) return Forbid();

            rule.IsActive = !rule.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRow(int id)
        {
            var row = await _context.ConflictRuleRows
                .Include(r => r.ConflictRule).ThenInclude(cr => cr.Campaign).ThenInclude(c => c.Members)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (row == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanManageCampaign(row.ConflictRule.Campaign, user)) return Forbid();

            var ruleId = row.ConflictRuleId;
            _context.ConflictRuleRows.Remove(row);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = ruleId });
        }

        private async Task<bool> IsGlobalStaff(UserModel user)
        {
            return await _userManager.IsInRoleAsync(user, "Administrator") || await _userManager.IsInRoleAsync(user, "GameMaster");
        }

        private async Task<bool> CanViewCampaign(CampaignModel campaign, UserModel user)
        {
            if (await IsGlobalStaff(user)) return true;
            return campaign.GameMasterId == user.Id || campaign.Members.Any(m => m.UserId == user.Id);
        }

        private async Task<bool> CanManageCampaign(CampaignModel campaign, UserModel user)
        {
            if (await IsGlobalStaff(user)) return true;
            return campaign.GameMasterId == user.Id || campaign.Members.Any(m => m.UserId == user.Id && m.IsAdmin);
        }
    }
}
