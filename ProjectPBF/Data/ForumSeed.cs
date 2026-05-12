using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;

namespace ProjectPBF.Data
{
    public static class ForumSeed
    {
        public static void EnsureSeedData(ApplicationDbContext db)
        {
            if (db.ForumCategories.Any()) return;

            var cat = new ForumCategoryModel { Name = "Œwiat gry", Description = "G³ówne lokacje i informacje", Order = 0 };
            db.ForumCategories.Add(cat);
            db.SaveChanges();

            var forum = new ForumModel { CategoryId = cat.Id, Name = "Miasto - Rynek", Description = "Miejsce spotkañ", AccessLevel = Models.Enums.ForumAccessLevel.Public, Order = 0 };
            db.Forums.Add(forum);
            db.SaveChanges();
        }
    }
}