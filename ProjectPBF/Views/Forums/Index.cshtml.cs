using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;

namespace ProjectPBF.Views.Forums
{
    [Authorize(Roles = "MistrzGry,Administrator")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) => _db = db;

        public IList<ForumModel> Forums { get; set; } = new List<ForumModel>();

        public async Task OnGetAsync()
        {
            Forums = await _db.Forums
                              .Include(f => f.Category)
                              .AsNoTracking()
                              .OrderBy(f => f.CategoryId).ThenBy(f => f.Order)
                              .ToListAsync();
        }
    }
}
