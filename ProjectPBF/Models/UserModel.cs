using Microsoft.AspNetCore.Identity;
using ProjektPBF.Models;

namespace ProjectPBF.Models
{
    public class UserModel : IdentityUser<int>
    {
        public string Nick { get; set; }
        public string? AvatarUrl { get; set; }
        public UserRole Role { get; set; } = UserRole.Player;
    }
}