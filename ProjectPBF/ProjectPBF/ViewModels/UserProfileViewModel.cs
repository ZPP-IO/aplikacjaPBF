using ProjectPBF.Models;

namespace ProjectPBF.ViewModels
{
    public class UserProfileViewModel
    {
        public UserModel User { get; set; } = null!;
        public List<CharacterModel> Characters { get; set; } = new();
    }
}