using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Controllers
{
    [Authorize(Roles = "MistrzGry,Administrator")]
    public class ForumsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ForumsController(ApplicationDbContext db) => _db = db;

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var forums = await _db.Forums
                                  .Include(f => f.Category)
                                  .AsNoTracking()
                                  .OrderBy(f => f.CategoryId).ThenBy(f => f.Order)
                                  .ToListAsync();

            // load recent threads for each forum (top 5)
            foreach (var forum in forums)
            {
                var threads = await _db.ForumThreads
                    .Where(t => t.ForumId == forum.Id)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .AsNoTracking()
                    .ToListAsync();
                forum.Threads = threads;
            }

            return View("Browse", forums);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _db.ForumCategories.AsNoTracking().ToListAsync();
            if (!categories.Any())
            {
                var def = new ForumCategoryModel { Name = "Ogólne", Description = "Domyœlna kategoria forum", Order = 0 };
                _db.ForumCategories.Add(def);
                await _db.SaveChangesAsync();
                categories = await _db.ForumCategories.AsNoTracking().ToListAsync();
            }
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.AccessLevels = Enum.GetValues(typeof(ForumAccessLevel));
            return View(new ForumModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumModel forum)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.ForumCategories.AsNoTracking().ToListAsync(), "Id", "Name");
                ViewBag.AccessLevels = Enum.GetValues(typeof(ForumAccessLevel));
                return View(forum);
            }
            var exists = await _db.ForumCategories.AnyAsync(c => c.Id == forum.CategoryId);
            if (!exists)
            {
                ModelState.AddModelError("CategoryId", "Wybrana kategoria nie istnieje.");
                ViewBag.Categories = new SelectList(await _db.ForumCategories.AsNoTracking().ToListAsync(), "Id", "Name");
                ViewBag.AccessLevels = Enum.GetValues(typeof(ForumAccessLevel));
                return View(forum);
            }

            _db.Forums.Add(forum);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")] // Tylko Admin edytuje strukturê forów
        public async Task<IActionResult> Edit(int id)
        {
            var forum = await _db.Forums.FindAsync(id);
            if (forum == null) return NotFound();

            ViewBag.Categories = new SelectList(await _db.ForumCategories.ToListAsync(), "Id", "Name", forum.CategoryId);
            ViewBag.AccessLevels = Enum.GetValues(typeof(ForumAccessLevel));
            return View(forum);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(ForumModel forum)
        {
            if (!ModelState.IsValid) return View(forum);

            _db.Attach(forum).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var forum = await _db.Forums.FindAsync(id);
            if (forum != null)
            {
                _db.Forums.Remove(forum);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
