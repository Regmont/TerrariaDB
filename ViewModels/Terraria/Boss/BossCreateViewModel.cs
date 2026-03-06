using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.Boss
{
    public class BossCreateViewModel
    {
        [Required(ErrorMessage = "Boss name is required")]
        [StringLength(50, ErrorMessage = "Boss name cannot exceed 50 characters")]
        public string BossName { get; set; } = string.Empty;

        public string? SummonItemId { get; set; }

        public List<BossDropCreateViewModel> BossDrops { get; set; } = new();
        public List<BossPartCreateViewModel> BossParts { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
        public List<SelectListItem> AvailableEnemies { get; set; } = new();
    }

    public class BossDropCreateViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }

    public class BossPartCreateViewModel
    {
        [Required(ErrorMessage = "Part name is required")]
        [StringLength(50, ErrorMessage = "Part name cannot exceed 50 characters")]
        public string PartName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string? Description { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public List<BossStageCreateViewModel> Stages { get; set; } = new();
    }

    public class BossStageCreateViewModel
    {
        [Required(ErrorMessage = "Sprite is required")]
        [StringLength(500, ErrorMessage = "Sprite path cannot exceed 500 characters")]
        public string Sprite { get; set; } = string.Empty;

        public int Hp { get; set; }

        public int Defense { get; set; }

        [Range(-500, 1000, ErrorMessage = "Entity ID must be between -500 and 1000")]
        public short EntityId { get; set; }

        public int ContactDamage { get; set; }

        public List<BossStageEnemyCreateViewModel> SpawnedEnemies { get; set; } = new();
        public List<BossStageDropCreateViewModel> Drops { get; set; } = new();
    }

    public class BossStageEnemyCreateViewModel
    {
        public string EnemyId { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }

    public class BossStageDropCreateViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
