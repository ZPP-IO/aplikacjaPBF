using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserModel, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CharacterModel> CharacterModels { get; set; } = null!;
        public DbSet<CampaignModel> CampaignModels { get; set; } = null!;
        public DbSet<CampaignCharacterModel> CampaignCharacterModels { get; set; } = null!;
        public DbSet<CampaignMemberModel> CampaignMembers { get; set; } = null!;
        public DbSet<CampaignStatisticModel> CampaignStatistics { get; set; } = null!;
        public DbSet<CharacterStatisticValueModel> CharacterStatisticValues { get; set; } = null!;
        public DbSet<CharacterSkillModel> CharacterSkills { get; set; } = null!;
        public DbSet<InventoryItemModel> InventoryItems { get; set; } = null!;
        public DbSet<CharacterDevelopmentLogModel> CharacterDevelopmentLogs { get; set; } = null!;
        public DbSet<CampaignClassModel> CampaignClasses { get; set; } = null!;
        public DbSet<CampaignItemTemplateModel> CampaignItemTemplates { get; set; } = null!;
        public DbSet<CampaignSkillTemplateModel> CampaignSkillTemplates { get; set; } = null!;
        public DbSet<ConflictRuleModel> ConflictRules { get; set; } = null!;
        public DbSet<ConflictRuleRowModel> ConflictRuleRows { get; set; } = null!;
        public DbSet<MissionProposalModel> MissionProposals { get; set; } = null!;
        public DbSet<MissionReviewModel> MissionReviews { get; set; } = null!;
        public DbSet<CalendarEventModel> CalendarEvents { get; set; } = null!;

        // Nowe DbSety sesji i forum
        public DbSet<SessionModel> SessionModels { get; set; } = null!;
        public DbSet<SessionMemberModel> SessionMembers { get; set; } = null!;
        public DbSet<SessionThreadModel> SessionThreads { get; set; } = null!;
        public DbSet<SessionPostModel> SessionPosts { get; set; } = null!;

        public DbSet<ForumCategoryModel> ForumCategories { get; set; } = null!;
        public DbSet<ForumModel> Forums { get; set; } = null!;
        public DbSet<ForumThreadModel> ForumThreads { get; set; } = null!;
        public DbSet<ForumPostModel> ForumPosts { get; set; } = null!;
        public DbSet<ForumPostRevisionModel> ForumPostRevisions { get; set; } = null!;
        public DbSet<ActivityLogModel> ActivityLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserModel>(entity =>
            {
                entity.Property(x => x.Nick).IsRequired().HasMaxLength(30);
                entity.Property(x => x.AvatarUrl).HasColumnType("nvarchar(max)");
                entity.Property(x => x.Bio).HasMaxLength(1000);
                entity.Property(x => x.AccountStatus).HasConversion<int>().HasDefaultValue(AccountStatus.Pending);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.Nick).IsUnique();
                entity.HasIndex(x => x.AccountStatus);

                entity.HasOne(x => x.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CharacterModel>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(60);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(4000);
                entity.Property(x => x.AvatarUrl).HasColumnType("nvarchar(max)");
                entity.Property(x => x.Status).HasConversion<int>().HasDefaultValue(CharacterStatus.Pending);
                entity.Property(x => x.AvailableStatisticPoints).HasDefaultValue(0);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => new { x.UserId, x.Status });

                entity.HasOne(x => x.User)
                    .WithMany(x => x.Characters)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CampaignClass)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignClassId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<CampaignModel>(entity =>
            {
                entity.Property(x => x.Title).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(4000);
                entity.Property(x => x.StartingPoints).HasDefaultValue(20);
                entity.Property(x => x.StatisticPointsPerLevel).HasDefaultValue(5);
                entity.Property(x => x.Status).HasConversion<int>().HasDefaultValue(CampaignStatus.Draft);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.GameMasterId);
                entity.HasIndex(x => x.Status);

                entity.HasOne(x => x.GameMaster)
                    .WithMany(x => x.LedCampaigns)
                    .HasForeignKey(x => x.GameMasterId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CampaignCharacterModel>(entity =>
            {
                entity.Property(x => x.CampaignDescription).HasMaxLength(3000);
                entity.Property(x => x.Notes).HasMaxLength(3000);
                entity.Property(x => x.Status).HasConversion<int>().HasDefaultValue(ParticipationStatus.Pending);
                entity.Property(x => x.JoinedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.CampaignId);
                entity.HasIndex(x => x.CharacterId);
                entity.HasIndex(x => new { x.CampaignId, x.CharacterId }).IsUnique();

                entity.HasOne(x => x.Campaign)
                    .WithMany(x => x.CampaignCharacters)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Character)
                    .WithMany(x => x.CampaignCharacters)
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CampaignMemberModel>(entity =>
            {
                entity.Property(x => x.JoinedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.CampaignId);
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => new { x.CampaignId, x.UserId }).IsUnique();

                entity.HasOne(x => x.Campaign)
                    .WithMany(x => x.Members)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.CampaignMemberships)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CampaignStatisticModel>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(80);
                entity.Property(x => x.DefaultValue).HasDefaultValue(0);
                entity.HasIndex(x => x.CampaignId);

                entity.HasOne(x => x.Campaign)
                    .WithMany(x => x.Statistics)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CharacterStatisticValueModel>(entity =>
            {
                entity.HasIndex(x => x.CharacterId);
                entity.HasIndex(x => x.CampaignStatisticId);
                entity.HasIndex(x => new { x.CharacterId, x.CampaignStatisticId }).IsUnique();

                entity.HasOne(x => x.Character)
                    .WithMany(x => x.StatisticValues)
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Statistic)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignStatisticId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CharacterSkillModel>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(4000);
                entity.Property(x => x.Requirements).HasMaxLength(2000);
                entity.Property(x => x.Effect).HasMaxLength(4000);
                entity.Property(x => x.Cost).HasDefaultValue(0);
                entity.Property(x => x.Cooldown).HasDefaultValue(0);
                entity.Property(x => x.RequestedHistoryPointCost).HasDefaultValue(0);
                entity.Property(x => x.ReviewComment).HasMaxLength(1000);
                entity.Property(x => x.Kind).HasConversion<int>();
                entity.Property(x => x.Status).HasConversion<int>().HasDefaultValue(SkillStatus.Pending);
                entity.Property(x => x.SubmittedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.CharacterId);
                entity.HasIndex(x => x.CampaignSkillTemplateId);
                entity.HasIndex(x => x.Status);

                entity.HasOne(x => x.Character)
                    .WithMany(x => x.Skills)
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CampaignSkillTemplate)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignSkillTemplateId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.ReviewedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.ReviewedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<InventoryItemModel>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
                entity.Property(x => x.Description).HasMaxLength(2000);
                entity.Property(x => x.ItemType).HasMaxLength(80);
                entity.Property(x => x.Rarity).HasMaxLength(80);
                entity.Property(x => x.Effect).HasMaxLength(4000);
                entity.Property(x => x.Quantity).HasDefaultValue(1);
                entity.Property(x => x.Source).HasMaxLength(120);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.CharacterId);
                entity.HasIndex(x => x.CampaignItemTemplateId);

                entity.HasOne(x => x.Character)
                    .WithMany(x => x.InventoryItems)
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CampaignItemTemplate)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignItemTemplateId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.GrantedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.GrantedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CharacterDevelopmentLogModel>(entity =>
            {
                entity.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
                entity.Property(x => x.Source).HasMaxLength(120);
                entity.Property(x => x.StatisticPointsChange).HasDefaultValue(0);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.CharacterId);
                entity.HasIndex(x => x.CreatedAt);

                entity.HasOne(x => x.Character)
                    .WithMany(x => x.DevelopmentLogs)
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CampaignClassModel>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(4000);
                entity.Property(x => x.MainStatistic).HasMaxLength(120);
                entity.Property(x => x.StartingHealth).HasDefaultValue(10);
                entity.Property(x => x.StartingResource).HasDefaultValue(3);
                entity.Property(x => x.StartingGold).HasDefaultValue(0);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.CampaignId);
                entity.HasIndex(x => new { x.CampaignId, x.Name }).IsUnique();

                entity.HasOne(x => x.Campaign)
                    .WithMany(x => x.Classes)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CampaignItemTemplateModel>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(4000);
                entity.Property(x => x.ItemType).HasConversion<int>();
                entity.Property(x => x.Rarity).HasMaxLength(80);
                entity.Property(x => x.Effect).HasMaxLength(4000);
                entity.Property(x => x.Price).HasDefaultValue(0);
                entity.Property(x => x.IsAvailable).HasDefaultValue(true);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.CampaignId);
                entity.HasIndex(x => new { x.CampaignId, x.Name });

                entity.HasOne(x => x.Campaign)
                    .WithMany(x => x.ItemTemplates)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CampaignSkillTemplateModel>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(4000);
                entity.Property(x => x.Kind).HasConversion<int>();
                entity.Property(x => x.Requirements).HasMaxLength(2000);
                entity.Property(x => x.Cost).HasDefaultValue(0);
                entity.Property(x => x.Cooldown).HasDefaultValue(0);
                entity.Property(x => x.Effect).HasMaxLength(4000);
                entity.Property(x => x.IsAvailable).HasDefaultValue(true);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.CampaignId);
                entity.HasIndex(x => new { x.CampaignId, x.Name });

                entity.HasOne(x => x.Campaign)
                    .WithMany(x => x.SkillTemplates)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ConflictRuleModel>(entity =>
            {
                entity.Property(x => x.Title).IsRequired().HasMaxLength(150);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(6000);
                entity.Property(x => x.Formula).HasMaxLength(1000);
                entity.Property(x => x.Example).HasMaxLength(3000);
                entity.Property(x => x.Type).HasConversion<int>();
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.CampaignId);
                entity.HasIndex(x => x.Type);
                entity.HasIndex(x => x.IsActive);

                entity.HasOne(x => x.Campaign)
                    .WithMany(x => x.ConflictRules)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<ConflictRuleRowModel>(entity =>
            {
                entity.Property(x => x.Outcome).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Effect).HasMaxLength(3000);
                entity.Property(x => x.Order).HasDefaultValue(0);
                entity.HasIndex(x => x.ConflictRuleId);
                entity.HasIndex(x => new { x.ConflictRuleId, x.Order });

                entity.HasOne(x => x.ConflictRule)
                    .WithMany(x => x.Rows)
                    .HasForeignKey(x => x.ConflictRuleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<MissionProposalModel>(entity =>
            {
                entity.Property(x => x.Title).IsRequired().HasMaxLength(150);
                entity.Property(x => x.Summary).IsRequired().HasMaxLength(1000);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(8000);
                entity.Property(x => x.Objective).HasMaxLength(3000);
                entity.Property(x => x.SuggestedRewards).HasMaxLength(3000);
                entity.Property(x => x.Risks).HasMaxLength(3000);
                entity.Property(x => x.Status).HasConversion<int>().HasDefaultValue(MissionProposalStatus.Pending);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.CampaignId);
                entity.HasIndex(x => x.Status);
                entity.HasIndex(x => x.SubmittedByUserId);

                entity.HasOne(x => x.Campaign)
                    .WithMany(x => x.MissionProposals)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.SubmittedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.SubmittedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<MissionReviewModel>(entity =>
            {
                entity.Property(x => x.Score).HasDefaultValue(5);
                entity.Property(x => x.Decision).HasConversion<int>();
                entity.Property(x => x.Comment).IsRequired().HasMaxLength(3000);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.MissionProposalId);
                entity.HasIndex(x => x.ReviewerId);
                entity.HasIndex(x => new { x.MissionProposalId, x.ReviewerId }).IsUnique();

                entity.HasOne(x => x.MissionProposal)
                    .WithMany(x => x.Reviews)
                    .HasForeignKey(x => x.MissionProposalId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Reviewer)
                    .WithMany()
                    .HasForeignKey(x => x.ReviewerId)
                    .OnDelete(DeleteBehavior.NoAction);
            });



            builder.Entity<CalendarEventModel>(entity =>
            {
                entity.Property(x => x.Title).IsRequired().HasMaxLength(150);
                entity.Property(x => x.Description).HasMaxLength(4000);
                entity.Property(x => x.Location).HasMaxLength(150);
                entity.Property(x => x.Visibility).HasConversion<int>().HasDefaultValue(CalendarEventVisibility.Campaign);
                entity.Property(x => x.IsImportant).HasDefaultValue(false);
                entity.Property(x => x.IsCancelled).HasDefaultValue(false);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.EventDate);
                entity.HasIndex(x => x.Visibility);
                entity.HasIndex(x => x.CampaignId);
                entity.HasIndex(x => x.CreatedByUserId);

                entity.HasOne(x => x.Campaign)
                    .WithMany(x => x.CalendarEvents)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });



            builder.Entity<SessionModel>(entity =>
            {
                entity.Property(x => x.Title).IsRequired().HasMaxLength(150);
                entity.Property(x => x.Description).HasMaxLength(4000);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.CampaignId);

                entity.HasOne(x => x.Campaign)
                      .WithMany(x => x.Sessions)
                      .HasForeignKey(x => x.CampaignId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.GameMaster)
                      .WithMany()
                      .HasForeignKey(x => x.GameMasterId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<SessionMemberModel>(entity =>
            {
                entity.HasIndex(x => new { x.SessionId, x.UserId }).IsUnique();
                entity.Property(x => x.JoinedAt).HasDefaultValueSql("NULL");
                entity.Property(x => x.Status).HasConversion<int>().HasDefaultValue(SessionMembershipStatus.Pending);

                entity.HasOne(x => x.Session)
                      .WithMany(x => x.Members)
                      .HasForeignKey(x => x.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<SessionThreadModel>(entity =>
            {
                entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.SessionId);

                entity.HasOne(x => x.Session)
                      .WithMany(x => x.Threads)
                      .HasForeignKey(x => x.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(x => x.CreatedByUserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<SessionPostModel>(entity =>
            {
                entity.Property(x => x.Content).IsRequired().HasMaxLength(8000);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.ThreadId);

                entity.HasOne(x => x.Thread)
                      .WithMany(x => x.Posts)
                      .HasForeignKey(x => x.ThreadId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
            builder.Entity<ActivityLogModel>(entity =>
            {
                entity.Property(x => x.ActionType)
                    .HasConversion<int>();

                entity.Property(x => x.EntityType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Description)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.CampaignId);
                entity.HasIndex(x => x.CharacterId);
                entity.HasIndex(x => x.CreatedAt);
                entity.HasIndex(x => x.ActionType);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.ActivityLogs)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.Campaign)
                    .WithMany(x => x.ActivityLogs)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Character)
                    .WithMany(x => x.ActivityLogs)
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // indeksy wyszukiwania i wydajności
            builder.Entity<ForumThreadModel>().HasIndex(t => t.Title);
            builder.Entity<ForumThreadModel>().HasIndex(t => t.IsArchived);
            builder.Entity<ForumPostModel>().HasIndex(p => p.ThreadId);
            builder.Entity<ForumPostModel>().HasIndex(p => p.CreatedAt);
            builder.Entity<ForumModel>().HasIndex(f => new { f.CategoryId, f.Order });

            // ograniczenia długości i cascade rules mogę dopracować według Twoich preferencji
        }
    }
}
