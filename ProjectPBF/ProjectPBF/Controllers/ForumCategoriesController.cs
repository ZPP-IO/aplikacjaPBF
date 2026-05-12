using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;

namespace ProjectPBF.Controllers
{
    [Authorize(Roles = "MistrzGry,Administrator")]
    public class ForumCategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ForumCategoriesController(ApplicationDbContext db) => _db = db;

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var cats = await _db.ForumCategories.AsNoTracking().OrderBy(c => c.Order).ToListAsync();
            return View(cats);
        }

        public IActionResult Create()
        {
            return View(new ForumCategoryModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumCategoryModel category)
        {
            if (!ModelState.IsValid) return View(category);
            _db.ForumCategories.Add(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var c = await _db.ForumCategories.FindAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ForumCategoryModel category)
        {
            if (!ModelState.IsValid) return View(category);
            _db.Attach(category).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.ForumCategories.FindAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var c = await _db.ForumCategories.FindAsync(id);
            if (c != null)
            {
                _db.ForumCategories.Remove(c);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
