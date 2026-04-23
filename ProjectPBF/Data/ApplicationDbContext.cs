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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserModel>(entity =>
            {
                entity.Property(x => x.Nick)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(x => x.AvatarUrl)
                    .HasMaxLength(500);

                entity.Property(x => x.Bio)
                    .HasMaxLength(1000);

                entity.Property(x => x.AccountStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(AccountStatus.Pending);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(x => x.Nick)
                    .IsUnique();

                entity.HasIndex(x => x.AccountStatus);

                entity.HasOne(x => x.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CharacterModel>(entity =>
            {
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(60);

                entity.Property(x => x.Description)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.Property(x => x.AvatarUrl)
                    .HasMaxLength(500);

                entity.Property(x => x.Status)
                    .HasConversion<int>()
                    .HasDefaultValue(CharacterStatus.Pending);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => new { x.UserId, x.Status });

                entity.HasOne(x => x.User)
                    .WithMany(x => x.Characters)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override int SaveChanges()
        {
            UpdateDates();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateDates();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateDates()
        {
            var modifiedCharacters = ChangeTracker.Entries<CharacterModel>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in modifiedCharacters)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}