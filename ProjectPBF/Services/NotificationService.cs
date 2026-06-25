using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task NotifyAsync(
            int userId,
            NotificationType type,
            string message,
            string linkUrl,
            int? relatedThreadId = null,
            int? relatedMessageId = null)
        {
            var notification = new NotificationModel
            {
                UserId = userId,
                Type = type,
                Message = message,
                LinkUrl = linkUrl,
                RelatedThreadId = relatedThreadId,
                RelatedMessageId = relatedMessageId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // Powiadamia wszystkich autorow poprzednich postow w watku (poza autorem nowego posta)
        // o nowej odpowiedzi. Kazdy autor otrzymuje maksymalnie jedno powiadomienie per nowy post.
        public async Task NotifyThreadRepliersAsync(int threadId, int newPostAuthorUserId, string threadTitle)
        {
            var participantUserIds = await _context.ForumPosts
                .Where(p => p.ThreadId == threadId && !p.IsDeleted && p.UserId != null)
                .Select(p => p.UserId!)
                .Distinct()
                .ToListAsync();

            var newPostAuthorIdString = newPostAuthorUserId.ToString();

            foreach (var participantIdString in participantUserIds)
            {
                if (participantIdString == newPostAuthorIdString) continue;
                if (!int.TryParse(participantIdString, out var participantId)) continue;

                await NotifyAsync(
                    participantId,
                    NotificationType.NewReplyInThread,
                    $"Nowa odpowiedź w wątku: {threadTitle}",
                    $"/Threads/Details/{threadId}",
                    relatedThreadId: threadId
                );
            }
        }
    }
}