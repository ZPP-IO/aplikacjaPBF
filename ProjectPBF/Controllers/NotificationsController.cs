using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public NotificationsController(ApplicationDbContext db)
        {
            _db = db;
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var parsed) ? parsed : null;
        }

        // Pelna lista powiadomien (najnowsze pierwsze)
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Challenge();

            var notifications = await _db.Notifications
                .Where(n => n.UserId == currentUserId.Value)
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .ToListAsync();

            return View(notifications);
        }

        // Oznacza jedno powiadomienie jako przeczytane i przekierowuje na jego docelowy link
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Open(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Challenge();

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == currentUserId.Value);

            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _db.SaveChangesAsync();

            return Redirect(notification.LinkUrl);
        }

        // Oznacza wszystkie powiadomienia uzytkownika jako przeczytane
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Challenge();

            var unread = await _db.Notifications
                .Where(n => n.UserId == currentUserId.Value && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}