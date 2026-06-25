using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ReportsController(ApplicationDbContext db)
        {
            _db = db;
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var parsed) ? parsed : null;
        }

        // Buduje krotki podglad zglaszanej tresci, zapisywany na sztywno w zgloszeniu,
        // zeby admin widzial kontekst nawet po edycji/usunieciu orginalu.
        private async Task<(bool found, string snapshot)> BuildSnapshotAsync(ReportTargetType targetType, int targetId)
        {
            switch (targetType)
            {
                case ReportTargetType.ForumPost:
                    var post = await _db.ForumPosts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == targetId);
                    if (post == null) return (false, string.Empty);
                    var postContent = post.Content.Length > 200 ? post.Content.Substring(0, 200) + "..." : post.Content;
                    return (true, $"Post od {post.AuthorDisplayName}: \"{postContent}\"");

                case ReportTargetType.ForumThread:
                    var thread = await _db.ForumThreads.AsNoTracking().FirstOrDefaultAsync(t => t.Id == targetId);
                    if (thread == null) return (false, string.Empty);
                    return (true, $"Watek: \"{thread.Title}\"");

                case ReportTargetType.PrivateMessage:
                    var message = await _db.PrivateMessages.AsNoTracking()
                        .Include(m => m.Sender)
                        .FirstOrDefaultAsync(m => m.Id == targetId);
                    if (message == null) return (false, string.Empty);
                    var msgContent = message.Content.Length > 200 ? message.Content.Substring(0, 200) + "..." : message.Content;
                    return (true, $"Wiadomosc od {message.Sender?.Nick ?? "?"}: \"{msgContent}\"");

                default:
                    return (false, string.Empty);
            }
        }

        // Wyznacza adres, pod ktorym admin moze zobaczyc zglaszany obiekt w jego naturalnym kontekscie.
        private async Task<string?> BuildTargetLinkAsync(ReportTargetType targetType, int targetId)
        {
            switch (targetType)
            {
                case ReportTargetType.ForumPost:
                    var post = await _db.ForumPosts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == targetId);
                    return post == null ? null : Url.Action("Details", "Threads", new { id = post.ThreadId });

                case ReportTargetType.ForumThread:
                    return Url.Action("Details", "Threads", new { id = targetId });

                case ReportTargetType.PrivateMessage:
                    var message = await _db.PrivateMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == targetId);
                    return message == null ? null : Url.Action("Conversation", "Messages", new { userId = message.SenderId });

                default:
                    return null;
            }
        }

        // GET: formularz zgloszenia konkretnego posta / watku / wiadomosci
        public async Task<IActionResult> Create(ReportTargetType targetType, int targetId, string? returnUrl)
        {
            var (found, snapshot) = await BuildSnapshotAsync(targetType, targetId);
            if (!found) return NotFound();

            ViewBag.TargetType = targetType;
            ViewBag.TargetId = targetId;
            ViewBag.Snapshot = snapshot;
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReportTargetType targetType, int targetId, string reason, string? returnUrl)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Challenge();

            if (string.IsNullOrWhiteSpace(reason))
            {
                ModelState.AddModelError(string.Empty, "Podaj powod zgloszenia.");
            }

            var (found, snapshot) = await BuildSnapshotAsync(targetType, targetId);
            if (!found) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.TargetType = targetType;
                ViewBag.TargetId = targetId;
                ViewBag.Snapshot = snapshot;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var report = new ReportModel
            {
                TargetType = targetType,
                TargetId = targetId,
                TargetSnapshot = snapshot,
                ReporterUserId = currentUserId.Value,
                Reason = reason.Trim(),
                CreatedAt = DateTime.UtcNow,
                Status = ReportStatus.New
            };

            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            ViewBag.ReturnUrl = returnUrl;
            return View("Confirmation");
        }

        // ── Panel administracyjny zgloszen ──

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index(ReportStatus? status)
        {
            var query = _db.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReviewedByUser)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var links = new System.Collections.Generic.Dictionary<int, string?>();
            foreach (var r in reports)
            {
                links[r.Id] = await BuildTargetLinkAsync(r.TargetType, r.TargetId);
            }

            ViewBag.TargetLinks = links;
            ViewBag.SelectedStatus = status;
            ViewBag.NewCount = await _db.Reports.CountAsync(r => r.Status == ReportStatus.New);

            return View(reports);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int id, ReportStatus status, string? adminNote)
        {
            var report = await _db.Reports.FindAsync(id);
            if (report == null) return NotFound();

            var currentUserId = GetCurrentUserId();

            report.Status = status;
            report.AdminNote = adminNote;
            report.ReviewedByUserId = currentUserId;
            report.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { status = (ReportStatus?)null });
        }
    }
}
