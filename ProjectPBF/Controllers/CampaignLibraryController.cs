using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class CampaignLibraryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public CampaignLibraryController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int campaignId)
        {
            var campaign = await _context.CampaignModels
                .Include(c => c.GameMaster)
                .Include(c => c.Members)
                .Include(c => c.Classes)
                .Include(c => c.ItemTemplates)
                .Include(c => c.SkillTemplates)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null) return NotFound();

            ViewBag.CanManageCampaignLibrary = await CanManageCampaign(campaign);
            return View(campaign);
        }

        public async Task<IActionResult> CreateClass(int campaignId)
        {
            var campaign = await GetCampaignIfCanManage(campaignId);
            if (campaign == null) return Forbid();

            ViewBag.CampaignTitle = campaign.Title;
            return View(new CampaignClassModel { CampaignId = campaignId, StartingHealth = 10, StartingResource = 3, IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClass(CampaignClassModel model)
        {
            var campaign = await GetCampaignIfCanManage(model.CampaignId);
            if (campaign == null) return Forbid();

            ModelState.Remove(nameof(CampaignClassModel.Campaign));

            if (!ModelState.IsValid)
            {
                ViewBag.CampaignTitle = campaign.Title;
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            _context.CampaignClasses.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { campaignId = model.CampaignId });
        }

        public async Task<IActionResult> CreateItem(int campaignId)
        {
            var campaign = await GetCampaignIfCanManage(campaignId);
            if (campaign == null) return Forbid();

            ViewBag.CampaignTitle = campaign.Title;
            return View(new CampaignItemTemplateModel { CampaignId = campaignId, IsAvailable = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(CampaignItemTemplateModel model)
        {
            var campaign = await GetCampaignIfCanManage(model.CampaignId);
            if (campaign == null) return Forbid();

            ModelState.Remove(nameof(CampaignItemTemplateModel.Campaign));

            if (!ModelState.IsValid)
            {
                ViewBag.CampaignTitle = campaign.Title;
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            _context.CampaignItemTemplates.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { campaignId = model.CampaignId });
        }

        public async Task<IActionResult> CreateSkill(int campaignId)
        {
            var campaign = await GetCampaignIfCanManage(campaignId);
            if (campaign == null) return Forbid();

            ViewBag.CampaignTitle = campaign.Title;
            return View(new CampaignSkillTemplateModel { CampaignId = campaignId, IsAvailable = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSkill(CampaignSkillTemplateModel model)
        {
            var campaign = await GetCampaignIfCanManage(model.CampaignId);
            if (campaign == null) return Forbid();

            ModelState.Remove(nameof(CampaignSkillTemplateModel.Campaign));

            if (!ModelState.IsValid)
            {
                ViewBag.CampaignTitle = campaign.Title;
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            _context.CampaignSkillTemplates.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { campaignId = model.CampaignId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var item = await _context.CampaignClasses
                .Include(x => x.Campaign).ThenInclude(c => c.Members)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null) return NotFound();
            if (!await CanManageCampaign(item.Campaign)) return Forbid();

            var campaignId = item.CampaignId;
            _context.CampaignClasses.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { campaignId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.CampaignItemTemplates
                .Include(x => x.Campaign).ThenInclude(c => c.Members)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null) return NotFound();
            if (!await CanManageCampaign(item.Campaign)) return Forbid();

            var campaignId = item.CampaignId;
            _context.CampaignItemTemplates.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { campaignId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var item = await _context.CampaignSkillTemplates
                .Include(x => x.Campaign).ThenInclude(c => c.Members)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null) return NotFound();
            if (!await CanManageCampaign(item.Campaign)) return Forbid();

            var campaignId = item.CampaignId;
            _context.CampaignSkillTemplates.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { campaignId });
        }

        private async Task<CampaignModel?> GetCampaignIfCanManage(int campaignId)
        {
            var campaign = await _context.CampaignModels
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null) return null;
            return await CanManageCampaign(campaign) ? campaign : null;
        }

        private async Task<bool> CanManageCampaign(CampaignModel campaign)
        {
            var userIdText = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdText) || !int.TryParse(userIdText, out var userId)) return false;

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return false;

            var isSystemAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
            var isGameMaster = campaign.GameMasterId == userId;
            var isCampaignAdmin = campaign.Members.Any(m => m.UserId == userId && m.IsAdmin);

            return isSystemAdmin || isGameMaster || isCampaignAdmin;
        }
    }
}
