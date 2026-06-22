using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class InventoryItemModel
    {
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public CharacterModel Character { get; set; } = null!;

        [Display(Name = "Wzorzec z biblioteki kampanii")]
        public int? CampaignItemTemplateId { get; set; }
        public CampaignItemTemplateModel? CampaignItemTemplate { get; set; }

        [Required]
        [MaxLength(120)]
        [Display(Name = "Nazwa przedmiotu")]
        public string Name { get; set; } = null!;

        [MaxLength(2000)]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [MaxLength(80)]
        [Display(Name = "Typ")]
        public string? ItemType { get; set; }

        [MaxLength(80)]
        [Display(Name = "Rzadkość")]
        public string? Rarity { get; set; }

        [MaxLength(4000)]
        [Display(Name = "Efekt mechaniczny")]
        public string? Effect { get; set; }

        [Range(1, 999)]
        [Display(Name = "Ilość")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Założony / aktywny")]
        public bool IsEquipped { get; set; }

        [MaxLength(120)]
        [Display(Name = "Źródło")]
        public string? Source { get; set; }

        public int? GrantedByUserId { get; set; }
        public UserModel? GrantedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
