using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;

namespace ProjectPBF.ViewComponents
{
    // Dzwoneczek powiadomien w naglowku - pokazuje licznik nieprzeczytanych
    // i kilka najnowszych powiadomien dla zalogowanego uzytkownika.
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public NotificationBellViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var raw = UserClaimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(raw, out var currentUserId))
            {
                return View(new NotificationBellViewModel());
            }

            var recent = await _db.Notifications
                .Where(n => n.UserId == currentUserId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(8)
                .ToListAsync();

            var unreadCount = await _db.Notifications
                .CountAsync(n => n.UserId == currentUserId && !n.IsRead);

            return View(new NotificationBellViewModel
            {
                Recent = recent,
                UnreadCount = unreadCount
            });
        }
    }

    public class NotificationBellViewModel
    {
        public System.Collections.Generic.List<ProjectPBF.Models.NotificationModel> Recent { get; set; } = new();
        public int UnreadCount { get; set; }
    }
}