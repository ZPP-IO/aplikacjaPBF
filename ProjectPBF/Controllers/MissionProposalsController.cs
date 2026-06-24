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
    public class MissionProposalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public MissionProposalsController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? campaignId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var isReviewer = await CanReviewAny(user);

            var query = _context.MissionProposals
                .Include(m => m.Campaign)
                .Include(m => m.SubmittedByUser)
                .Include(m => m.Reviews)
                .AsQueryable();

            if (campaignId.HasValue)
            {
                query = query.Where(m => m.CampaignId == campaignId.Value);
            }

            if (!isReviewer)
            {
                var userId = user.Id;
                query = query.Where(m => m.SubmittedByUserId == userId || m.Campaign.GameMasterId == userId || m.Campaign.Members.Any(cm => cm.UserId == userId));
            }

            var missions = await query
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.CampaignId = campaignId;
            ViewBag.CanReview = isReviewer;
            return View(missions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var mission = await _context.MissionProposals
                .Include(m => m.Campaign).ThenInclude(c => c.Members)
                .Include(m => m.SubmittedByUser)
                .Include(m => m.Reviews).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mission == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!await CanViewCampaign(mission.Campaign, user) && mission.SubmittedByUserId != user.Id) return Forbid();

            ViewBag.CanReview = await CanReviewCampaign(mission.Campaign, user);
            ViewBag.CanManage = await CanManageCampaign(mission.Campaign, user);
            return View(mission);
        }

        public async Task<IActionResult> Create(int campaignId)
        {
            var campaign = await _context.CampaignModels
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanViewCampaign(campaign, user)) return Forbid();

            ViewBag.Campaign = campaign;
            return View(new MissionProposalModel { CampaignId = campaignId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int campaignId, string title, string summary, string description, string? objective, string? suggestedRewards, string? risks)
        {
            var campaign = await _context.CampaignModels
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanViewCampaign(campaign, user)) return Forbid();

            if (string.IsNullOrWhiteSpace(title)) ModelState.AddModelError(nameof(title), "Podaj tytuł misji.");
            if (string.IsNullOrWhiteSpace(summary)) ModelState.AddModelError(nameof(summary), "Podaj krótki opis.");
            if (string.IsNullOrWhiteSpace(description)) ModelState.AddModelError(nameof(description), "Podaj opis fabularny.");

            if (!ModelState.IsValid)
            {
                ViewBag.Campaign = campaign;
                return View(new MissionProposalModel
                {
                    CampaignId = campaignId,
                    Title = title,
                    Summary = summary,
                    Description = description,
                    Objective = objective,
                    SuggestedRewards = suggestedRewards,
                    Risks = risks
                });
            }

            var proposal = new MissionProposalModel
            {
                CampaignId = campaignId,
                Title = title.Trim(),
                Summary = summary.Trim(),
                Description = description.Trim(),
                Objective = string.IsNullOrWhiteSpace(objective) ? null : objective.Trim(),
                SuggestedRewards = string.IsNullOrWhiteSpace(suggestedRewards) ? null : suggestedRewards.Trim(),
                Risks = string.IsNullOrWhiteSpace(risks) ? null : risks.Trim(),
                SubmittedByUserId = user.Id,
                Status = MissionProposalStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.MissionProposals.Add(proposal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = proposal.Id });
        }

        public async Task<IActionResult> Review(int id)
        {
            var mission = await _context.MissionProposals
                .Include(m => m.Campaign).ThenInclude(c => c.Members)
                .Include(m => m.SubmittedByUser)
                .Include(m => m.Reviews).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mission == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanReviewCampaign(mission.Campaign, user)) return Forbid();

            var existingReview = mission.Reviews.FirstOrDefault(r => r.ReviewerId == user.Id);
            ViewBag.ExistingReview = existingReview;
            return View(mission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, int score, MissionReviewDecision decision, string comment)
        {
            var mission = await _context.MissionProposals
                .Include(m => m.Campaign).ThenInclude(c => c.Members)
                .Include(m => m.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mission == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanReviewCampaign(mission.Campaign, user)) return Forbid();

            if (score < 1 || score > 10) ModelState.AddModelError(nameof(score), "Ocena musi być od 1 do 10.");
            if (string.IsNullOrWhiteSpace(comment)) ModelState.AddModelError(nameof(comment), "Dodaj komentarz do werdyktu.");

            if (!ModelState.IsValid)
            {
                mission = await _context.MissionProposals
                    .Include(m => m.Campaign).ThenInclude(c => c.Members)
                    .Include(m => m.SubmittedByUser)
                    .Include(m => m.Reviews).ThenInclude(r => r.Reviewer)
                    .FirstAsync(m => m.Id == id);
                ViewBag.ExistingReview = mission.Reviews.FirstOrDefault(r => r.ReviewerId == user.Id);
                return View(mission);
            }

            var review = mission.Reviews.FirstOrDefault(r => r.ReviewerId == user.Id);
            if (review == null)
            {
                review = new MissionReviewModel
                {
                    MissionProposalId = mission.Id,
                    ReviewerId = user.Id
                };
                _context.MissionReviews.Add(review);
            }

            review.Score = score;
            review.Decision = decision;
            review.Comment = comment.Trim();
            review.CreatedAt = DateTime.UtcNow;

            mission.Status = decision switch
            {
                MissionReviewDecision.Approve => MissionProposalStatus.Accepted,
                MissionReviewDecision.NeedsChanges => MissionProposalStatus.NeedsChanges,
                MissionReviewDecision.Reject => MissionProposalStatus.Rejected,
                _ => mission.Status
            };
            mission.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = mission.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            var mission = await _context.MissionProposals
                .Include(m => m.Campaign).ThenInclude(c => c.Members)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mission == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await CanManageCampaign(mission.Campaign, user)) return Forbid();

            mission.Status = MissionProposalStatus.Completed;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<bool> CanReviewAny(UserModel user)
        {
            return await _userManager.IsInRoleAsync(user, "Administrator")
                || await _userManager.IsInRoleAsync(user, "GameMaster")
                || await _userManager.IsInRoleAsync(user, "Werdyktujacy")
                || await _userManager.IsInRoleAsync(user, "Werdyktujący")
                || await _userManager.IsInRoleAsync(user, "Judge")
                || await _userManager.IsInRoleAsync(user, "Verdicting");
        }

        private async Task<bool> CanViewCampaign(CampaignModel campaign, UserModel user)
        {
            if (await CanReviewAny(user)) return true;
            return campaign.GameMasterId == user.Id || campaign.Members.Any(m => m.UserId == user.Id);
        }

        private async Task<bool> CanManageCampaign(CampaignModel campaign, UserModel user)
        {
            if (await _userManager.IsInRoleAsync(user, "Administrator") || await _userManager.IsInRoleAsync(user, "GameMaster")) return true;
            return campaign.GameMasterId == user.Id || campaign.Members.Any(m => m.UserId == user.Id && m.IsAdmin);
        }

        private async Task<bool> CanReviewCampaign(CampaignModel campaign, UserModel user)
        {
            if (await CanReviewAny(user)) return true;
            return campaign.GameMasterId == user.Id || campaign.Members.Any(m => m.UserId == user.Id && m.IsAdmin);
        }
    }
}
