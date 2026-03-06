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
        public string? Description { get; set; } = string.Empty;

        public List<EnemyStageViewModel> Stages { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
    }

    public class EnemyStageViewModel
    {
        [Required(ErrorMessage = "Sprite is required")]
        [StringLength(500, ErrorMessage = "Sprite path cannot exceed 500 characters")]
        public string Sprite { get; set; } = string.Empty;

        public int Hp { get; set; }

        public int Defense { get; set; }

        public short EntityId { get; set; }

        public int ContactDamage { get; set; }

        public List<EnemyDropCreateViewModel> Drops { get; set; } = new();
    }

    public class EnemyDropCreateViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
