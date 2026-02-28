using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.Enemy
{
    public class EnemyCreateViewModel
    {
        [Required(ErrorMessage = "Enemy name is required")]
        [StringLength(50, ErrorMessage = "Enemy name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        public List<EnemyStageViewModel> Stages { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
    }

    public class EnemyStageViewModel
    {
        [Required(ErrorMessage = "Sprite is required")]
        [StringLength(500, ErrorMessage = "Sprite path cannot exceed 500 characters")]
        public string Sprite { get; set; } = string.Empty;

        [Range(0, 30000, ErrorMessage = "HP must be between 0 and 30,000")]
        public int Hp { get; set; }

        [Range(0, 10000, ErrorMessage = "Defense must be between 0 and 10,000")]
        public int Defense { get; set; }

        public short EntityId { get; set; }

        [Range(0, 1000, ErrorMessage = "Contact damage must be between 0 and 1,000")]
        public int ContactDamage { get; set; }

        public List<EnemyDropCreateViewModel> Drops { get; set; } = new();
    }

    public class EnemyDropCreateViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
        public int Quantity { get; set; }
    }
}
