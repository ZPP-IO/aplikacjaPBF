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
                .Include(c => c.CampaignCharacters).ThenInclude(cc => cc.Character)
                .Include(c => c.Sessions).ThenInclude(s => s.GameMaster)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null) return NotFound();

            // Flaga: czy bie¿¹cy u¿ytkownik (zalogowany) mo¿e tworzyæ sesjê (GM lub admin kampanii)
            var userIdText = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userIdText) && int.TryParse(userIdText, out var userId))
            {
                var isGameMaster = campaign.GameMasterId == userId;
                var isAdminMember = campaign.Members.Any(m => m.UserId == userId && m.IsAdmin);
                ViewBag.CanCreateSession = isGameMaster || isAdminMember;
            }
            else
            {
                ViewBag.CanCreateSession = false;
            }

            // Przekazujemy posortowan¹ listê sesji (najnowsze pierwsze)
            ViewBag.Sessions = campaign.Sessions
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            return View(campaign);
        }
    }
}
