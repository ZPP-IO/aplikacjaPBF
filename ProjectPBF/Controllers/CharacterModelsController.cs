using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;
using ProjectPBF.ViewModels;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class CharacterModelsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public CharacterModelsController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString)) return Challenge();

            var userId = int.Parse(userIdString);

            var myCharacters = await _context.CharacterModels
                .Include(c => c.User)
                .Include(c => c.CampaignCharacters).ThenInclude(cc => cc.Campaign)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(myCharacters);
        }

        public async Task<IActionResult> Browse()
        {
            var characters = await _context.CharacterModels
                .Include(c => c.User)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(characters);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var characterModel = await _context.CharacterModels
                .Include(c => c.User)
                .Include(c => c.CampaignClass)
                .Include(c => c.CampaignCharacters).ThenInclude(cc => cc.Campaign)
                .Include(c => c.StatisticValues).ThenInclude(sv => sv.Statistic)
                .Include(c => c.Skills).ThenInclude(s => s.CampaignSkillTemplate)
                .Include(c => c.Skills).ThenInclude(s => s.ReviewedByUser)
                .Include(c => c.InventoryItems).ThenInclude(i => i.CampaignItemTemplate)
                .Include(c => c.InventoryItems).ThenInclude(i => i.GrantedByUser)
                .Include(c => c.DevelopmentLogs).ThenInclude(dl => dl.CreatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (characterModel == null) return NotFound();

            return View(characterModel);
        }

        // Teraz przycisk "Stwórz nową postać" najpierw pokazuje wybór kampanii.
        // Postać może powstać dopiero po dołączeniu do kampanii, bo statystyki są brane z jej schematu.
        public async Task<IActionResult> Create()
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString)) return Challenge();

            var userId = int.Parse(userIdString);

            var campaigns = await _context.CampaignModels
                .Include(c => c.GameMaster)
                .Include(c => c.Members)
                .Include(c => c.Statistics)
                .Include(c => c.CampaignCharacters).ThenInclude(cc => cc.Character)
                .Where(c => c.Status == CampaignStatus.Active || c.GameMasterId == userId || c.Members.Any(m => m.UserId == userId))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var model = new CharacterCampaignChoiceViewModel
            {
                Campaigns = campaigns.Select(c => new CharacterCampaignChoiceItemViewModel
                {
                    CampaignId = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    GameMasterName = c.GameMaster?.Nick ?? "Brak danych",
                    StartingPoints = c.StartingPoints,
                    StatisticPointsPerLevel = c.StatisticPointsPerLevel,
                    StatisticsSummary = c.Statistics.Any()
                        ? string.Join(", ", c.Statistics.OrderBy(s => s.Id).Select(s => s.Name))
                        : "Brak statystyk",
                    IsOwner = c.GameMasterId == userId,
                    IsMember = c.GameMasterId == userId || c.Members.Any(m => m.UserId == userId),
                    HasCharacterInCampaign = c.CampaignCharacters.Any(cc => cc.Character != null && cc.Character.UserId == userId)
                }).ToList()
            };

            return View(model);
        }

        // Stara akcja POST zostaje jako awaryjna obsługa starego formularza, ale normalny flow używa CreateForCampaign.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Strength,Agility,Intelligence,AvatarUrl")] CharacterModel characterModel)
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString)) return Challenge();

            characterModel.UserId = int.Parse(userIdString);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            characterModel.Status = CharacterStatus.Pending;

            if (await _userManager.IsInRoleAsync(currentUser, "Administrator") ||
                await _userManager.IsInRoleAsync(currentUser, "GameMaster"))
            {
                characterModel.Status = CharacterStatus.Approved;
            }

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ValidateAndNormalizeAvatarUrl(characterModel.AvatarUrl, nameof(CharacterModel.AvatarUrl), value => characterModel.AvatarUrl = value);

            if (ModelState.IsValid)
            {
                _context.Add(characterModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(characterModel);
        }

        public async Task<IActionResult> CreateForCampaign(int campaignId)
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString)) return Challenge();
            var userId = int.Parse(userIdString);

            var campaign = await _context.CampaignModels
                .Include(c => c.Members)
                .Include(c => c.Statistics)
                .Include(c => c.Classes)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null) return NotFound();

            var isMember = campaign.GameMasterId == userId || campaign.Members.Any(m => m.UserId == userId);
            if (!isMember)
            {
                TempData["Error"] = "Najpierw dołącz do kampanii, dopiero potem możesz tworzyć do niej postać.";
                return RedirectToAction(nameof(Create));
            }

            if (!campaign.Statistics.Any())
            {
                TempData["Error"] = "Ta kampania nie ma jeszcze ustawionych statystyk. MG musi najpierw dodać schemat postaci.";
                return RedirectToAction(nameof(Create));
            }

            var model = new CharacterCreateInCampaignViewModel
            {
                CampaignId = campaign.Id,
                CampaignTitle = campaign.Title,
                StartingPoints = campaign.StartingPoints,
                ClassOptions = BuildClassOptions(campaign.Classes),
                Statistics = campaign.Statistics
                    .OrderBy(s => s.Id)
                    .Select(s => new CharacterStatisticInputViewModel
                    {
                        StatisticId = s.Id,
                        Name = s.Name,
                        Value = s.DefaultValue
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForCampaign(CharacterCreateInCampaignViewModel model)
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString)) return Challenge();

            var userId = int.Parse(userIdString);

            var campaign = await _context.CampaignModels
                .Include(c => c.Members)
                .Include(c => c.Statistics)
                .Include(c => c.Classes)
                .FirstOrDefaultAsync(c => c.Id == model.CampaignId);

            if (campaign == null) return NotFound();

            var isCampaignMember = campaign.GameMasterId == userId || campaign.Members.Any(m => m.UserId == userId);
            if (!isCampaignMember)
            {
                ModelState.AddModelError(string.Empty, "Nie możesz stworzyć postaci w kampanii, do której nie dołączono.");
            }

            model.CampaignTitle = campaign.Title;
            model.StartingPoints = campaign.StartingPoints;
            model.ClassOptions = BuildClassOptions(campaign.Classes);

            var validStats = campaign.Statistics.OrderBy(s => s.Id).ToList();
            var postedStats = model.Statistics ?? new List<CharacterStatisticInputViewModel>();

            model.Statistics = validStats.Select(stat =>
            {
                var posted = postedStats.FirstOrDefault(x => x.StatisticId == stat.Id);
                return new CharacterStatisticInputViewModel
                {
                    StatisticId = stat.Id,
                    Name = stat.Name,
                    Value = posted?.Value ?? stat.DefaultValue
                };
            }).ToList();

            if (model.CampaignClassId.HasValue && !campaign.Classes.Any(c => c.Id == model.CampaignClassId.Value && c.IsActive))
            {
                ModelState.AddModelError(nameof(CharacterCreateInCampaignViewModel.CampaignClassId), "Wybrana klasa nie należy do tej kampanii albo jest nieaktywna.");
            }

            ValidateAndNormalizeAvatarUrl(model.AvatarUrl, nameof(CharacterCreateInCampaignViewModel.AvatarUrl), value => model.AvatarUrl = value);

            foreach (var input in model.Statistics)
            {
                if (input.Value < 0)
                {
                    ModelState.AddModelError(string.Empty, "Statystyki nie mogą być ujemne.");
                    break;
                }
            }

            var totalPoints = model.Statistics.Sum(s => s.Value);
            if (totalPoints > campaign.StartingPoints)
            {
                ModelState.AddModelError(string.Empty, $"Przekroczono pulę punktów. Rozdano {totalPoints}/{campaign.StartingPoints}. Zmniejsz statystyki przed zapisaniem.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var isStaff = await _userManager.IsInRoleAsync(currentUser, "Administrator") ||
                          await _userManager.IsInRoleAsync(currentUser, "GameMaster") ||
                          campaign.GameMasterId == userId;

            var character = new CharacterModel
            {
                Name = model.Name,
                Description = model.Description,
                AvatarUrl = model.AvatarUrl,
                CampaignClassId = model.CampaignClassId,
                UserId = userId,
                Status = isStaff ? CharacterStatus.Approved : CharacterStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                AvailableStatisticPoints = 0,
                Strength = GetPostedStatValue(model.Statistics, validStats, "Siła"),
                Agility = GetPostedStatValue(model.Statistics, validStats, "Zręczność"),
                Intelligence = GetPostedStatValue(model.Statistics, validStats, "Inteligencja")
            };

            _context.CharacterModels.Add(character);
            await _context.SaveChangesAsync();

            _context.CampaignCharacterModels.Add(new CampaignCharacterModel
            {
                CampaignId = campaign.Id,
                CharacterId = character.Id,
                Status = isStaff ? ParticipationStatus.Accepted : ParticipationStatus.Pending,
                JoinedAt = DateTime.UtcNow
            });

            foreach (var stat in validStats)
            {
                var posted = model.Statistics.FirstOrDefault(x => x.StatisticId == stat.Id);
                _context.CharacterStatisticValues.Add(new CharacterStatisticValueModel
                {
                    CharacterId = character.Id,
                    CampaignStatisticId = stat.Id,
                    Value = posted?.Value ?? stat.DefaultValue
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = character.Id });
        }

        [Authorize(Roles = "GameMaster,Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var characterModel = await _context.CharacterModels.FindAsync(id);
            if (characterModel == null) return NotFound();

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Nick", characterModel.UserId);
            return View(characterModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GameMaster,Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Strength,Agility,Intelligence,AvatarUrl,Status,Experience,HistoryPoints,Level,AvailableStatisticPoints,UserId")] CharacterModel characterModel)
        {
            if (id != characterModel.Id) return NotFound();

            ModelState.Remove("User");
            ValidateAndNormalizeAvatarUrl(characterModel.AvatarUrl, nameof(CharacterModel.AvatarUrl), value => characterModel.AvatarUrl = value);

            if (ModelState.IsValid)
            {
                try
                {
                    characterModel.UpdatedAt = DateTime.UtcNow;
                    _context.Update(characterModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CharacterModelExists(characterModel.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Nick", characterModel.UserId);
            return View(characterModel);
        }

        [Authorize(Roles = "GameMaster,Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var characterModel = await _context.CharacterModels
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (characterModel == null) return NotFound();

            return View(characterModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GameMaster,Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var characterModel = await _context.CharacterModels.FindAsync(id);
            if (characterModel != null)
            {
                _context.CharacterModels.Remove(characterModel);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private static List<SelectListItem> BuildClassOptions(IEnumerable<CampaignClassModel> classes)
        {
            var options = new List<SelectListItem>
            {
                new SelectListItem("Brak klasy / do ustalenia z MG", "")
            };

            options.AddRange(classes
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem(c.Name, c.Id.ToString())));

            return options;
        }

        private static int GetPostedStatValue(List<CharacterStatisticInputViewModel> postedStats, List<CampaignStatisticModel> validStats, string statName)
        {
            var stat = validStats.FirstOrDefault(s => s.Name.Equals(statName, StringComparison.OrdinalIgnoreCase));
            if (stat == null) return 0;

            return postedStats.FirstOrDefault(s => s.StatisticId == stat.Id)?.Value ?? stat.DefaultValue;
        }

        private void ValidateAndNormalizeAvatarUrl(string? avatarUrl, string modelStateKey, Action<string?> setValue)
        {
            var cleaned = avatarUrl?.Trim();

            if (string.IsNullOrWhiteSpace(cleaned))
            {
                setValue(null);
                return;
            }

            if (cleaned.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(modelStateKey, "Nie wklejaj obrazka w formacie base64. Wklej zwykły link do obrazka albo zostaw pole puste.");
                return;
            }

            if (cleaned.Length > 2048)
            {
                ModelState.AddModelError(modelStateKey, "Link do avatara jest za długi. Maksymalnie 2048 znaków.");
                return;
            }

            if (cleaned.StartsWith("/", StringComparison.Ordinal))
            {
                setValue(cleaned);
                return;
            }

            if (!Uri.TryCreate(cleaned, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                ModelState.AddModelError(modelStateKey, "Avatar musi być linkiem http/https albo lokalną ścieżką zaczynającą się od /. Możesz też zostawić to pole puste.");
                return;
            }

            setValue(cleaned);
        }

        private bool CharacterModelExists(int id)
        {
            return _context.CharacterModels.Any(e => e.Id == id);
        }
    }
}
