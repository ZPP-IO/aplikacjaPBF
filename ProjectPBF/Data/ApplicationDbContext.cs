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
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => new { x.UserId, x.Status });

                entity.HasOne(x => x.User)
                    .WithMany(x => x.Characters)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CampaignModel>(entity =>
            {
                entity.Property(x => x.Title).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(4000);
                entity.Property(x => x.StartingPoints).HasDefaultValue(20);
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

                entity.HasOne(x => x.Character)
                    .WithMany(x => x.StatisticValues)
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Statistic)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignStatisticId)
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

            // indeksy wyszukiwania i wydajnoœci
            builder.Entity<ForumThreadModel>().HasIndex(t => t.Title);
            builder.Entity<ForumPostModel>().HasIndex(p => p.ThreadId);
            builder.Entity<ForumPostModel>().HasIndex(p => p.CreatedAt);
            builder.Entity<ForumModel>().HasIndex(f => new { f.CategoryId, f.Order });

            // ograniczenia d³ugoœci i cascade rules mogê dopracowaæ wed³ug Twoich preferencji
        }
    }
}
