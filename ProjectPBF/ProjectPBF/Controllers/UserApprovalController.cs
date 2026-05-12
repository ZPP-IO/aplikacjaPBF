using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Controllers
{
    [Authorize(Roles = "Administrator,GameMaster")]
    public class UserApprovalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public UserApprovalController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var pendingUsers = await _context.Users
                .Where(u => u.AccountStatus == AccountStatus.Pending)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            return View(pendingUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentAdmin = await _userManager.GetUserAsync(User);
            if (currentAdmin == null)
            {
                return Challenge();
            }

            user.AccountStatus = AccountStatus.Approved;
            user.ApprovedAt = DateTime.UtcNow;
            user.ApprovedByUserId = currentAdmin.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentAdmin = await _userManager.GetUserAsync(User);
            if (currentAdmin == null)
            {
                return Challenge();
            }

            user.AccountStatus = AccountStatus.Rejected;
            user.ApprovedAt = DateTime.UtcNow;
            user.ApprovedByUserId = currentAdmin.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}