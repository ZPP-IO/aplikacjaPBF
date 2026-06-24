using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;

namespace ProjectPBF.Controllers
{
    [Authorize(Roles = "Administrator,GameMaster")]
    public class ActivityLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.ActivityLogs
                .Include(x => x.User)
                .Include(x => x.Campaign)
                .Include(x => x.Character)
                .OrderByDescending(x => x.CreatedAt)
                .Take(100)
                .ToListAsync();

            return View(logs);
        }
    }
}