using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;
using ProjectPBF.ViewModels;
using System.Security.Claims;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        // Użytkownik jest "aktywny" jeśli był widziany w ciągu ostatnich 15 minut.
        // "Away" jeśli między 15 a 60 minut temu. Powyżej – Offline.
        private static readonly TimeSpan ActiveThreshold = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan AwayThreshold = TimeSpan.FromMinutes(60);

        public UsersController(ApplicationDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Users — widok społeczności
        public async Task<IActionResult> Index(string? search, string? status)
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? currentUserId = int.TryParse(currentUserIdStr, out var cid) ? cid : null;

            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.AccountStatus == AccountStatus.Approved
                         && (!currentUserId.HasValue || u.Id != currentUserId.Value));

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.Nick.Contains(search));

            var users = await query
                .Select(u => new
                {
                    u.Id,
                    u.Nick,
                    u.AvatarUrl,
                    u.Bio,
                    u.CreatedAt,
                    u.LastSeenAt,
                    u.PresenceStatus,
                    CharacterCount = u.Characters.Count(),
                    CampaignCount = u.CampaignMemberships.Count() + u.LedCampaigns.Count()
                })
                .ToListAsync();

            var now = DateTime.UtcNow;

            var cards = users.Select(u => new CommunityUserCardViewModel
            {
                Id = u.Id,
                Nick = u.Nick,
                AvatarUrl = u.AvatarUrl,
                Bio = u.Bio,
                CreatedAt = u.CreatedAt,
                CharacterCount = u.CharacterCount,
                CampaignCount = u.CampaignCount,
                ManualStatus = u.PresenceStatus,
                LastSeenAt = u.LastSeenAt,
                OnlineStatus = ComputeStatus(u.LastSeenAt, u.PresenceStatus, now)
            }).ToList();

            // Filtr statusu po obliczeniu (robi się w pamięci, bo zależy od czasu)
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ComputedOnlineStatus>(status, out var statusEnum))
                cards = cards.Where(c => c.OnlineStatus == statusEnum).ToList();

            // Sortowanie: najpierw online/busy, potem away, na końcu offline; w ramach grupy – po nicku
            cards = cards
                .OrderBy(c => c.OnlineStatus switch
                {
                    ComputedOnlineStatus.Online => 0,
                    ComputedOnlineStatus.Busy => 1,
                    ComputedOnlineStatus.Away => 2,
                    _ => 3
                })
                .ThenBy(c => c.Nick)
                .ToList();

            PresenceStatus? currentUserStatus = null;
            if (currentUserId.HasValue)
            {
                var me = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == currentUserId.Value)
                    .Select(u => new { u.PresenceStatus })
                    .FirstOrDefaultAsync();
                currentUserStatus = me?.PresenceStatus;
            }

            var vm = new CommunityIndexViewModel
            {
                Users = cards,
                SearchQuery = search,
                StatusFilter = status,
                TotalCount = cards.Count,
                OnlineCount = cards.Count(c => c.OnlineStatus is ComputedOnlineStatus.Online or ComputedOnlineStatus.Busy),
                CurrentUserStatus = currentUserStatus
            };

            return View(vm);
        }

        // Profil jednego użytkownika
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var characters = await _context.CharacterModels
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var viewModel = new UserProfileViewModel
            {
                User = user,
                Characters = characters
            };

            return View(viewModel);
        }

        // POST: /Users/SetStatus — AJAX endpoint do zmiany statusu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus([FromBody] SetStatusRequest request)
        {
            if (!Enum.IsDefined(typeof(PresenceStatus), request.Status))
                return BadRequest(new { error = "Nieprawidłowy status." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            user.PresenceStatus = (PresenceStatus)request.Status;
            await _userManager.UpdateAsync(user);

            return Ok(new { status = user.PresenceStatus.ToString() });
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> MakeAdmin(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Content("Nie znaleziono użytkownika");
            await _userManager.AddToRoleAsync(user, "Administrator");
            return Content("Dodano administratora");
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> MakeGM(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Content("Nie znaleziono użytkownika");
            await _userManager.AddToRoleAsync(user, "GameMaster");
            return Content("Dodano MG");
        }

        // --- helpers ---

        private static ComputedOnlineStatus ComputeStatus(DateTime? lastSeen, PresenceStatus manual, DateTime now)
        {
            if (lastSeen == null) return ComputedOnlineStatus.Offline;

            var elapsed = now - lastSeen.Value;

            if (elapsed > AwayThreshold) return ComputedOnlineStatus.Offline;
            if (elapsed > ActiveThreshold) return ComputedOnlineStatus.Away;

            // Jest aktywny – respektujemy ręczny wybór
            return manual switch
            {
                PresenceStatus.Busy => ComputedOnlineStatus.Busy,
                PresenceStatus.Unavailable => ComputedOnlineStatus.Offline,
                _ => ComputedOnlineStatus.Online
            };
        }
    }

    public class SetStatusRequest
    {
        public int Status { get; set; }
    }
}
