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
    public class CharacterSkillsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public CharacterSkillsController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userIdText = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdText)) return Challenge();
            var userId = int.Parse(userIdText);

            var skills = await _context.CharacterSkills
                .Include(s => s.Character)
                .Include(s => s.CampaignSkillTemplate)
                .Where(s => s.Character.UserId == userId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return View(skills);
        }

        public async Task<IActionResult> Create(int characterId)
        {
            var character = await GetCharacterIfCanEdit(characterId);
            if (character == null) return NotFound();

            await PrepareCreateView(character, new CharacterSkillModel { CharacterId = characterId });
            return View(new CharacterSkillModel { CharacterId = characterId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CharacterId,CampaignSkillTemplateId,Name,Description,Kind,Requirements,Effect,Cost,Cooldown,RequestedHistoryPointCost")] CharacterSkillModel model)
        {
            var character = await GetCharacterIfCanEdit(model.CharacterId);
            if (character == null) return NotFound();

            ModelState.Remove(nameof(CharacterSkillModel.Character));
            ModelState.Remove(nameof(CharacterSkillModel.CampaignSkillTemplate));
            ModelState.Remove(nameof(CharacterSkillModel.ReviewedByUser));
            ModelState.Remove(nameof(CharacterSkillModel.ReviewComment));

            CampaignSkillTemplateModel? template = null;
            if (model.CampaignSkillTemplateId.HasValue)
            {
                var campaignIds = character.CampaignCharacters.Select(cc => cc.CampaignId).ToList();
                template = await _context.CampaignSkillTemplates
                    .FirstOrDefaultAsync(t => t.Id == model.CampaignSkillTemplateId.Value && campaignIds.Contains(t.CampaignId) && t.IsAvailable);

                if (template == null)
                {
                    ModelState.AddModelError(nameof(CharacterSkillModel.CampaignSkillTemplateId), "Wybrany wzorzec nie należy do kampanii tej postaci albo jest nieaktywny.");
                }
                else
                {
                    // Gdy gracz wybiera gotowy skill z biblioteki MG, przepisujemy brakujące pola.
                    model.Name = string.IsNullOrWhiteSpace(model.Name) ? template.Name : model.Name.Trim();
                    model.Description = string.IsNullOrWhiteSpace(model.Description) ? template.Description : model.Description.Trim();
                    model.Requirements = string.IsNullOrWhiteSpace(model.Requirements) ? template.Requirements : model.Requirements.Trim();
                    model.Effect = string.IsNullOrWhiteSpace(model.Effect) ? template.Effect : model.Effect.Trim();
                    model.Cost = model.Cost == 0 ? template.Cost : model.Cost;
                    model.Cooldown = model.Cooldown == 0 ? template.Cooldown : model.Cooldown;
                    model.Kind = MapTemplateKind(template.Kind);
                }
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(CharacterSkillModel.Name), "Podaj nazwę umiejętności albo wybierz wzorzec z biblioteki kampanii.");
            }

            if (string.IsNullOrWhiteSpace(model.Description))
            {
                ModelState.AddModelError(nameof(CharacterSkillModel.Description), "Podaj opis działania albo wybierz wzorzec z biblioteki kampanii.");
            }

            if (!ModelState.IsValid)
            {
                await PrepareCreateView(character, model);
                return View(model);
            }

            model.Name = model.Name.Trim();
            model.Description = model.Description.Trim();
            model.Requirements = string.IsNullOrWhiteSpace(model.Requirements) ? null : model.Requirements.Trim();
            model.Effect = string.IsNullOrWhiteSpace(model.Effect) ? null : model.Effect.Trim();
            model.Status = SkillStatus.Pending;
            model.SubmittedAt = DateTime.UtcNow;

            _context.CharacterSkills.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "CharacterModels", new { id = model.CharacterId });
        }

        [Authorize(Roles = "GameMaster,Administrator")]
        public async Task<IActionResult> Pending()
        {
            var skills = await _context.CharacterSkills
                .Include(s => s.Character).ThenInclude(c => c.User)
                .Include(s => s.Character).ThenInclude(c => c.CampaignCharacters).ThenInclude(cc => cc.Campaign)
                .Include(s => s.CampaignSkillTemplate)
                .Where(s => s.Status == SkillStatus.Pending)
                .OrderBy(s => s.SubmittedAt)
                .ToListAsync();

            return View(skills);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GameMaster,Administrator")]
        public async Task<IActionResult> Approve(int id, string? reviewComment)
        {
            return await Review(id, SkillStatus.Approved, reviewComment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GameMaster,Administrator")]
        public async Task<IActionResult> Reject(int id, string? reviewComment)
        {
            return await Review(id, SkillStatus.Rejected, reviewComment);
        }

        private async Task<IActionResult> Review(int id, SkillStatus status, string? reviewComment)
        {
            var skill = await _context.CharacterSkills.FindAsync(id);
            if (skill == null) return NotFound();

            var reviewerIdText = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(reviewerIdText)) return Challenge();

            skill.Status = status;
            skill.ReviewComment = string.IsNullOrWhiteSpace(reviewComment) ? null : reviewComment.Trim();
            skill.ReviewedAt = DateTime.UtcNow;
            skill.ReviewedByUserId = int.Parse(reviewerIdText);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Pending));
        }

        private async Task PrepareCreateView(CharacterModel character, CharacterSkillModel model)
        {
            ViewBag.CharacterName = character.Name;
            ViewBag.CampaignTitle = character.CampaignCharacters.FirstOrDefault()?.Campaign?.Title ?? "Brak kampanii";

            var campaignIds = character.CampaignCharacters.Select(cc => cc.CampaignId).ToList();
            var templates = await _context.CampaignSkillTemplates
                .Where(t => campaignIds.Contains(t.CampaignId) && t.IsAvailable)
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.SkillTemplates = templates;
            ViewBag.SelectedTemplateId = model.CampaignSkillTemplateId;
        }

        private async Task<CharacterModel?> GetCharacterIfCanEdit(int characterId)
        {
            var character = await _context.CharacterModels
                .Include(c => c.CampaignCharacters)
                    .ThenInclude(cc => cc.Campaign)
                        .ThenInclude(campaign => campaign.Members)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null) return null;
            return await CanEditCharacter(character) ? character : null;
        }

        private async Task<bool> CanEditCharacter(CharacterModel character)
        {
            var userIdText = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdText)) return false;

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return false;

            var userId = int.Parse(userIdText);
            var isOwner = character.UserId == userId;
            var isStaff = await _userManager.IsInRoleAsync(user, "GameMaster") ||
                          await _userManager.IsInRoleAsync(user, "Administrator");
            var isCampaignOwnerOrAdmin = character.CampaignCharacters.Any(cc =>
                cc.Campaign != null &&
                (cc.Campaign.GameMasterId == userId || cc.Campaign.Members.Any(m => m.UserId == userId && m.IsAdmin)));

            return isOwner || isStaff || isCampaignOwnerOrAdmin;
        }

        private static SkillKind MapTemplateKind(CampaignSkillTemplateKind kind)
        {
            return kind == CampaignSkillTemplateKind.Technique ? SkillKind.Technique : SkillKind.Skill;
        }
    }
}
