using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.ViewModels
{
    public class CharacterStatisticInputViewModel
    {
        public int StatisticId { get; set; }
        public string Name { get; set; } = string.Empty;

        [Range(0, 999)]
        public int Value { get; set; }
    }
}
