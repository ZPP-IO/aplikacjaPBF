using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;

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

        // Moje postacie
        public async Task<IActionResult> Index()
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Challenge();
            }

            var userId = int.Parse(userIdString);

            var myCharacters = await _context.CharacterModels
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(myCharacters);
        }

        // Wszystkie postacie do podglądu
        public async Task<IActionResult> Browse()
        {
            var characters = await _context.CharacterModels
                .Include(c => c.User)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(characters);
        }

        // Szczegóły jednej postaci
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var characterModel = await _context.CharacterModels
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (characterModel == null)
            {
                return NotFound();
            }

            return View(characterModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Strength,Agility,Intelligence,AvatarUrl")] CharacterModel characterModel)
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Challenge();
            }

            characterModel.UserId = int.Parse(userIdString);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // Domyślnie postać oczekuje na akceptację
            characterModel.Status = CharacterStatus.Pending;

            // MG / Administrator mogą od razu utworzyć zaakceptowaną postać
            if (await _userManager.IsInRoleAsync(currentUser, "Administrator") ||
                await _userManager.IsInRoleAsync(currentUser, "GameMaster"))
            {
                characterModel.Status = CharacterStatus.Approved;
            }

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                _context.Add(characterModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(characterModel);
        }

        [Authorize(Roles = "GameMaster,Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var characterModel = await _context.CharacterModels.FindAsync(id);
            if (characterModel == null)
            {
                return NotFound();
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Nick", characterModel.UserId);
            return View(characterModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GameMaster,Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Strength,Agility,Intelligence,AvatarUrl,Status,UserId")] CharacterModel characterModel)
        {
            if (id != characterModel.Id)
            {
                return NotFound();
            }

            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(characterModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CharacterModelExists(characterModel.Id))
                    {
                        return NotFound();
                    }

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
            if (id == null)
            {
                return NotFound();
            }

            var characterModel = await _context.CharacterModels
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (characterModel == null)
            {
                return NotFound();
            }

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

        private bool CharacterModelExists(int id)
        {
            return _context.CharacterModels.Any(e => e.Id == id);
        }
    }
}