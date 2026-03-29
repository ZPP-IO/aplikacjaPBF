using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ProjectPBF.Data;
using ProjectPBF.Models;

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
            var userId = int.Parse(_userManager.GetUserId(User));
            var myCharacters = _context.CharacterModels
                .Include(c => c.User)
                .Where(c => c.UserId == userId);

            return View(await myCharacters.ToListAsync());
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
                return Challenge();

            characterModel.UserId = int.Parse(userIdString);

            characterModel.IsAccepted = false;

            // MG/Admin mogą od razu zaakceptować postać
            var currentUser = await _userManager.GetUserAsync(User);
            if (await _userManager.IsInRoleAsync(currentUser, "Admin") ||
                await _userManager.IsInRoleAsync(currentUser, "GameMaster"))
            {
                characterModel.IsAccepted = true;
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

        [Authorize(Roles = "GameMaster,Admin")]
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

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", characterModel.UserId);
            return View(characterModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GameMaster,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Strength,Agility,Intelligence,AvatarUrl,IsAccepted,UserId")] CharacterModel characterModel)
        {
            if (id != characterModel.Id)
            {
                return NotFound();
            }

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
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", characterModel.UserId);
            return View(characterModel);
        }

        [Authorize(Roles = "GameMaster,Admin")]
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
        [Authorize(Roles = "GameMaster,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var characterModel = await _context.CharacterModels.FindAsync(id);
            if (characterModel != null)
            {
                _context.CharacterModels.Remove(characterModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CharacterModelExists(int id)
        {
            return _context.CharacterModels.Any(e => e.Id == id);
        }
    }
}