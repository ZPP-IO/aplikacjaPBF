using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Services
{
    public class ActivityLogService
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            int? userId,
            ActivityActionType actionType,
            string entityType,
            int? entityId,
            string description,
            int? campaignId = null,
            int? characterId = null)
        {
            var log = new ActivityLogModel
            {
                UserId = userId,
                CampaignId = campaignId,
                CharacterId = characterId,
                ActionType = actionType,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}