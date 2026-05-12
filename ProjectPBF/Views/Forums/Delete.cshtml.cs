using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectPBF.Data;

namespace ProjectPBF.Views.Forums
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
            var f = await _db.Forums.FindAsync(id);
            if (f == null) return NotFound();
            Id = f.Id;
            Name = f.Name;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var f = await _db.Forums.FindAsync(Id);
            if (f != null)
            {
                _db.Forums.Remove(f);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage("Index");
        }
    }
}
