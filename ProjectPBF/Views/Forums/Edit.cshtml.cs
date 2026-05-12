using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Views.Forums
{
    [Authorize(Roles = "MistrzGry,Administrator")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public EditModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public ForumModel Forum { get; set; } = new();

        public SelectList? Categories { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var f = await _db.Forums.FindAsync(id);
            if (f == null) return NotFound();
            Forum = f;
            Categories = new SelectList(await _db.ForumCategories.AsNoTracking().ToListAsync(), "Id", "Name", Forum.CategoryId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = new SelectList(await _db.ForumCategories.AsNoTracking().ToListAsync(), "Id", "Name", Forum.CategoryId);
                return Page();
            }

            _db.Attach(Forum).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
