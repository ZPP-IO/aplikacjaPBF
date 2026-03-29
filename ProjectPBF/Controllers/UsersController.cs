using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }

        // Lista wszystkich użytkowników
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderBy(u => u.Nick)
                .ToListAsync();

            return View(users);
        }

        // Profil jednego użytkownika
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var characters = await _context.CharacterModels
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var viewModel = new UserProfileViewModel
            {
                User = user,
                Characters = characters
            };

            return View(viewModel);

        }
        public async Task<IActionResult> MakeAdmin(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Content("Nie znaleziono użytkownika");
            }

            await _userManager.AddToRoleAsync(user, "ADMIN");

            return Content("Dodano admina");
        }
        // Odpal aplikację i wejdź w https://localhost:xxxx/Users/MakeAdmin?email=twoj@email.com
        // i masz admina
        public async Task<IActionResult> MakeGM(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Content("Nie znaleziono użytkownika");
            }

            await _userManager.AddToRoleAsync(user, "GM");

            return Content("Dodano GMa");
        }


    }
}