using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class RankingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RankingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? campaignId)
        {
            var query = _context.CharacterModels
                .Include(c => c.User)
                .Include(c => c.CampaignCharacters).ThenInclude(cc => cc.Campaign)
                .AsQueryable();

            if (campaignId.HasValue)
            {
                query = query.Where(c => c.CampaignCharacters.Any(cc => cc.CampaignId == campaignId.Value));
            }

            var ranking = await query
                .OrderByDescending(c => c.Experience)
                .ThenByDescending(c => c.HistoryPoints)
                .ThenByDescending(c => c.Level)
                .ThenBy(c => c.Name)
                .ToListAsync();

            ViewBag.Campaigns = await _context.CampaignModels.OrderBy(c => c.Title).ToListAsync();
            ViewBag.SelectedCampaignId = campaignId;

            return View(ranking);
        }
    }
}
