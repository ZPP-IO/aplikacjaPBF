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
using ProjectPBF.Services;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly NotificationService _notificationService;

        public MessagesController(ApplicationDbContext db, NotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var parsed) ? parsed : null;
        }

        // Skrzynka odbiorcza: lista konwersacji (jedna pozycja per rozmówca, z ostatnią wiadomością)
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Challenge();

            var myMessages = await _db.PrivateMessages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m => m.SenderId == currentUserId.Value || m.RecipientId == currentUserId.Value)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            var conversations = myMessages
                .GroupBy(m => m.SenderId == currentUserId.Value ? m.RecipientId : m.SenderId)
                .Select(g => new ConversationSummary
                {
                    OtherUser = (g.First().SenderId == currentUserId.Value ? g.First().Recipient : g.First().Sender)!,
                    LastMessage = g.First(),
                    UnreadCount = g.Count(m => m.RecipientId == currentUserId.Value && !m.IsRead)
                })
                .OrderByDescending(c => c.LastMessage.SentAt)
                .ToList();

            ViewBag.Conversations = conversations;
            return View();
        }

        // Konwersacja z jednym konkretnym uzytkownikiem
        public async Task<IActionResult> Conversation(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Challenge();

            var otherUser = await _db.Users.FindAsync(userId);
            if (otherUser == null) return NotFound();

            var messages = await _db.PrivateMessages
                .Where(m =>
                    (m.SenderId == currentUserId.Value && m.RecipientId == userId) ||
                    (m.SenderId == userId && m.RecipientId == currentUserId.Value))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // oznacz jako przeczytane wszystkie wiadomosci, ktore otrzymalismy w tej konwersacji
            var unread = messages.Where(m => m.RecipientId == currentUserId.Value && !m.IsRead).ToList();
            foreach (var m in unread)
            {
                m.IsRead = true;
                m.ReadAt = DateTime.UtcNow;
            }
            if (unread.Any())
            {
                await _db.SaveChangesAsync();
            }

            ViewBag.OtherUser = otherUser;
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int recipientId, string content)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Challenge();

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Conversation), new { userId = recipientId });
            }

            if (recipientId == currentUserId.Value)
            {
                return RedirectToAction(nameof(Conversation), new { userId = recipientId });
            }

            var recipient = await _db.Users.FindAsync(recipientId);
            if (recipient == null) return NotFound();

            var message = new PrivateMessageModel
            {
                SenderId = currentUserId.Value,
                RecipientId = recipientId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow
            };

            _db.PrivateMessages.Add(message);
            await _db.SaveChangesAsync();

            var sender = await _db.Users.FindAsync(currentUserId.Value);
            var senderNick = sender?.Nick ?? "użytkownika";
            await _notificationService.NotifyAsync(
                recipientId,
                NotificationType.NewPrivateMessage,
                $"Nowa wiadomość od {senderNick}",
                $"/Messages/Conversation/{currentUserId.Value}",
                relatedMessageId: message.Id
            );

            return RedirectToAction(nameof(Conversation), new { userId = recipientId });
        }
    }

    // DTO uzywane do wyswietlenia listy konwersacji w skrzynce odbiorczej.
    public class ConversationSummary
    {
        public UserModel OtherUser { get; set; } = null!;
        public PrivateMessageModel LastMessage { get; set; } = null!;
        public int UnreadCount { get; set; }
    }
}