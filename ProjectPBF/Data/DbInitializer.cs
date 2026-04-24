using Microsoft.AspNetCore.Identity;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<UserModel>>();

            string[] roles = { "Administrator", "GameMaster", "Player" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>
                    {
                        Name = role,
                        NormalizedName = role.ToUpper()
                    });
                }
            }

            var adminEmail = "admin@local.test";
            var adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new UserModel
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Nick = "admin",
                    AvatarUrl = "",
                    AccountStatus = AccountStatus.Approved,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception("Nie udało się utworzyć konta admin: " + errors);
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Administrator"))
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
    }
}