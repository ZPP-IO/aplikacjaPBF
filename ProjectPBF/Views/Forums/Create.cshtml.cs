using System;
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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public CreateModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public ForumModel Forum { get; set; } = new();

        public SelectList? Categories { get; set; }
        public SelectList? AccessLevels { get; set; }

        public async Task OnGetAsync()
        {
            Categories = new SelectList(await _db.ForumCategories.AsNoTracking().ToListAsync(), "Id", "Name");
            AccessLevels = new SelectList(Enum.GetValues(typeof(ForumAccessLevel)));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            _db.Forums.Add(Forum);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}