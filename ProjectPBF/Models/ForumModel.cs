using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Models
{
    public class ForumModel
    {
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual ForumCategoryModel? Category { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public ForumAccessLevel AccessLevel { get; set; } = ForumAccessLevel.Public;

        public int Order { get; set; } = 0;

        public virtual ICollection<ForumThreadModel> Threads { get; set; } = new List<ForumThreadModel>();
    }
}