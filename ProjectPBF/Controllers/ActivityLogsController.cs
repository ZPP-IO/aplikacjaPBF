using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class ActivityLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public ActivityLogsController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Administrator");
            var isGameMaster = await _userManager.IsInRoleAsync(currentUser, "GameMaster");

            var query = _context.ActivityLogs
                .Include(x => x.User)
                .Include(x => x.Campaign)
                .Include(x => x.Character)
                .AsQueryable();

            if (!isAdmin && !isGameMaster)
            {
                query = query.Where(x => x.UserId == currentUser.Id);
            }

            var logs = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(200)
                .ToListAsync();

            return View(logs);
        }
    }
}