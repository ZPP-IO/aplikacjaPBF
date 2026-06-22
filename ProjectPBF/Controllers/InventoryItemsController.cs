using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class InventoryItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public InventoryItemsController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(int characterId)
        {
            var character = await GetCharacterIfCanEdit(characterId);
            if (character == null) return NotFound();

            await PrepareCreateView(character, new InventoryItemModel { CharacterId = characterId, Quantity = 1 });
            return View(new InventoryItemModel { CharacterId = characterId, Quantity = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CharacterId,CampaignItemTemplateId,Name,Description,ItemType,Rarity,Effect,Quantity,IsEquipped,Source")] InventoryItemModel model)
        {
            var character = await GetCharacterIfCanEdit(model.CharacterId);
            if (character == null) return NotFound();

            ModelState.Remove(nameof(InventoryItemModel.Character));
            ModelState.Remove(nameof(InventoryItemModel.CampaignItemTemplate));
            ModelState.Remove(nameof(InventoryItemModel.GrantedByUser));

            CampaignItemTemplateModel? template = null;
            if (model.CampaignItemTemplateId.HasValue)
            {
                var campaignIds = character.CampaignCharacters.Select(cc => cc.CampaignId).ToList();
                template = await _context.CampaignItemTemplates
                    .FirstOrDefaultAsync(t => t.Id == model.CampaignItemTemplateId.Value && campaignIds.Contains(t.CampaignId) && t.IsAvailable);

                if (template == null)
                {
                    ModelState.AddModelError(nameof(InventoryItemModel.CampaignItemTemplateId), "Wybrany przedmiot nie należy do kampanii tej postaci albo jest niedostępny.");
                }
                else
                {
                    model.Name = string.IsNullOrWhiteSpace(model.Name) ? template.Name : model.Name.Trim();
                    model.Description = string.IsNullOrWhiteSpace(model.Description) ? template.Description : model.Description.Trim();
                    model.ItemType = string.IsNullOrWhiteSpace(model.ItemType) ? template.ItemType.ToString() : model.ItemType.Trim();
                    model.Rarity = string.IsNullOrWhiteSpace(model.Rarity) ? template.Rarity : model.Rarity.Trim();
                    model.Effect = string.IsNullOrWhiteSpace(model.Effect) ? template.Effect : model.Effect.Trim();
                    model.Source = string.IsNullOrWhiteSpace(model.Source) ? "Biblioteka kampanii" : model.Source.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(InventoryItemModel.Name), "Podaj nazwę przedmiotu albo wybierz wzorzec z biblioteki kampanii.");
            }

            if (model.Quantity < 1)
            {
                ModelState.AddModelError(nameof(InventoryItemModel.Quantity), "Ilość musi być większa od 0.");
            }

            if (!ModelState.IsValid)
            {
                await PrepareCreateView(character, model);
                return View(model);
            }

            var userIdText = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdText)) return Challenge();

            model.Name = model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.ItemType = string.IsNullOrWhiteSpace(model.ItemType) ? null : model.ItemType.Trim();
            model.Rarity = string.IsNullOrWhiteSpace(model.Rarity) ? null : model.Rarity.Trim();
            model.Effect = string.IsNullOrWhiteSpace(model.Effect) ? null : model.Effect.Trim();
            model.Source = string.IsNullOrWhiteSpace(model.Source) ? null : model.Source.Trim();
            model.GrantedByUserId = int.Parse(userIdText);
            model.CreatedAt = DateTime.UtcNow;

            _context.InventoryItems.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "CharacterModels", new { id = model.CharacterId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEquipped(int id)
        {
            var item = await _context.InventoryItems
                .Include(i => i.Character)
                    .ThenInclude(c => c.CampaignCharacters)
                        .ThenInclude(cc => cc.Campaign)
                            .ThenInclude(campaign => campaign.Members)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();
            if (!await CanEditCharacter(item.Character)) return Forbid();

            item.IsEquipped = !item.IsEquipped;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "CharacterModels", new { id = item.CharacterId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.InventoryItems
                .Include(i => i.Character)
                    .ThenInclude(c => c.CampaignCharacters)
                        .ThenInclude(cc => cc.Campaign)
                            .ThenInclude(campaign => campaign.Members)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();
            if (!await CanEditCharacter(item.Character)) return Forbid();

            var characterId = item.CharacterId;
            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "CharacterModels", new { id = characterId });
        }

        private async Task PrepareCreateView(CharacterModel character, InventoryItemModel model)
        {
            ViewBag.CharacterName = character.Name;
            ViewBag.CampaignTitle = character.CampaignCharacters.FirstOrDefault()?.Campaign?.Title ?? "Brak kampanii";

            var campaignIds = character.CampaignCharacters.Select(cc => cc.CampaignId).ToList();
            var templates = await _context.CampaignItemTemplates
                .Where(t => campaignIds.Contains(t.CampaignId) && t.IsAvailable)
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.ItemTemplates = templates;
            ViewBag.SelectedTemplateId = model.CampaignItemTemplateId;
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
    }
}
