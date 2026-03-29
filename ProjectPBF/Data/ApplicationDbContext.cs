using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Models;

namespace ProjectPBF.Data;

public class ApplicationDbContext : IdentityDbContext<UserModel, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<CharacterModel> CharacterModels { get; set; }

}