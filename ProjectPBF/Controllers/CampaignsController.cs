using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;
using ProjectPBF.ViewModels;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class CampaignsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public CampaignsController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userIdText = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdText)) return Challenge();

            var userId = int.Parse(userIdText);

            var campaigns = await _context.CampaignModels
                .Include(c => c.GameMaster)
                .Include(c => c.Members)
                .Where(c => c.GameMasterId == userId || c.Members.Any(m => m.UserId == userId))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(campaigns);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Browse()
        {
            var campaigns = await _context.CampaignModels
                .Include(c => c.GameMaster)
                .Include(c => c.Members)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(campaigns);
        }

        public IActionResult Create()
        {
            return View(new CampaignCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CampaignCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userIdText = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdText)) return Challenge();

            var userId = int.Parse(userIdText);

            var campaign = new CampaignModel
            {
                Title = model.Title,
                Description = model.Description,
                StartingPoints = model.StartingPoints,
                StatisticPointsPerLevel = model.StatisticPointsPerLevel,
                GameMasterId = userId,
                Status = CampaignStatus.Active
            };

            var statistics = model.StatisticsText
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var stat in statistics)
            {
                campaign.Statistics.Add(new CampaignStatisticModel
                {
                    Name = stat,
                    DefaultValue = 0
                });
            }

            campaign.Members.Add(new CampaignMemberModel
            {
                UserId = userId,
                IsAdmin = true
            });

            _context.CampaignModels.Add(campaign);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = campaign.Id });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var campaign = await _context.CampaignModels
                .Include(c => c.GameMaster)
                .Include(c => c.Members).ThenInclude(m => m.User)
                .Include(c => c.Statistics)
                .Include(c => c.Classes)
                .Include(c => c.ItemTemplates)
                .Include(c => c.SkillTemplates)
                .Include(c => c.ConflictRules)
                .Include(c => c.MissionProposals)
                .Include(c => c.CalendarEvents)
                .Include(c => c.CampaignCharacters).ThenInclude(cc => cc.Character)
                .Include(c => c.Sessions).ThenInclude(s => s.GameMaster)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null) return NotFound();

            var userIdText = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userIdText) && int.TryParse(userIdText, out var userId))
            {
                var isGameMaster = campaign.GameMasterId == userId;
                var isAdminMember = campaign.Members.Any(m => m.UserId == userId && m.IsAdmin);
                var isMember = campaign.Members.Any(m => m.UserId == userId);

                ViewBag.CanCreateSession = isGameMaster || isAdminMember;
                ViewBag.CanManageCampaign = isGameMaster || isAdminMember;
                ViewBag.IsCampaignMember = isGameMaster || isMember;
                ViewBag.CanCreateCharacter = isGameMaster || isMember;
            }
            else
            {
                ViewBag.CanCreateSession = false;
                ViewBag.CanManageCampaign = false;
                ViewBag.IsCampaignMember = false;
                ViewBag.CanCreateCharacter = false;
            }

            ViewBag.Sessions = campaign.Sessions
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            return View(campaign);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id, bool createCharacter = false)
        {
            var userIdText = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdText)) return Challenge();

            var userId = int.Parse(userIdText);

            var campaign = await _context.CampaignModels
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null) return NotFound();

            var isOwner = campaign.GameMasterId == userId;
            var alreadyMember = campaign.Members.Any(m => m.UserId == userId);

            if (!isOwner && !alreadyMember)
            {
                _context.CampaignMembers.Add(new CampaignMemberModel
                {
                    CampaignId = campaign.Id,
                    UserId = userId,
                    IsAdmin = false,
                    JoinedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            if (createCharacter)
            {
                return RedirectToAction("CreateForCampaign", "CharacterModels", new { campaignId = campaign.Id });
            }

            return RedirectToAction(nameof(Details), new { id = campaign.Id });
        }
    }
}
