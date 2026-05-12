using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

using ProjectPBF.Data;

namespace ProjectPBF.Views.ForumCategories
{
    [Authorize(Roles = "MistrzGry,Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public DeleteModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public int Id { get; set; }

        public string? Name { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var c = await _db.ForumCategories.FindAsync(id);
            if (c == null) return NotFound();
            Id = c.Id;
            Name = c.Name;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var c = await _db.ForumCategories.FindAsync(Id);
            if (c != null)
            {
                _db.ForumCategories.Remove(c);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage("Index");
        }
    }
}