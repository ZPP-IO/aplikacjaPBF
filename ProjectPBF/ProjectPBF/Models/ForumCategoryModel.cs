using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectPBF.Models
{
    public class ForumCategoryModel
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int Order { get; set; } = 0;

        public virtual ICollection<ForumModel> Forums { get; set; } = new List<ForumModel>();
    }
}