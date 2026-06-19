using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;

namespace ProjectPBF.Views.ForumCategories
{
    [Authorize(Roles = "MistrzGry,Administrator")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) => _db = db;

        public IList<ForumCategoryModel> Categories { get; set; } = new List<ForumCategoryModel>();

        public async Task OnGetAsync()
        {
            Categories = await _db.ForumCategories
                                 .AsNoTracking()
                                 .OrderBy(c => c.Order)
                                 .ToListAsync();
        }
    }
}
