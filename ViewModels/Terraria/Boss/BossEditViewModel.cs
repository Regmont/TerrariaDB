using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.Boss
{
    public class BossEditViewModel
    {
        public string OriginalBossName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Boss name is required")]
        [StringLength(50, ErrorMessage = "Boss name cannot exceed 50 characters")]
        public string BossName { get; set; } = string.Empty;

        public string? SummonItemId { get; set; }

        public List<BossDropEditViewModel> BossDrops { get; set; } = new();
        public List<BossPartEditViewModel> BossParts { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
        public List<SelectListItem> AvailableEnemies { get; set; } = new();
    }

    public class BossDropEditViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        [Range(1, 150, ErrorMessage = "Quantity must be between 1 and 150")]
        public int Quantity { get; set; }
    }

    public class BossPartEditViewModel
    {
        [Required(ErrorMessage = "Part name is required")]
        [StringLength(50, ErrorMessage = "Part name cannot exceed 50 characters")]
        public string PartName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
        public int Quantity { get; set; }

        public List<BossStageEditViewModel> Stages { get; set; } = new();
    }

    public class BossStageEditViewModel
    {
        [Required(ErrorMessage = "Sprite is required")]
        [StringLength(500, ErrorMessage = "Sprite path cannot exceed 500 characters")]
        public string Sprite { get; set; } = string.Empty;

        [Range(0, 150000, ErrorMessage = "HP must be between 0 and 150,000")]
        public int Hp { get; set; }

        [Range(0, 100, ErrorMessage = "Defense must be between 0 and 100")]
        public int Defense { get; set; }

        [Range(-500, 1000, ErrorMessage = "Entity ID must be between -500 and 1000")]
        public short EntityId { get; set; }

        [Range(0, 500, ErrorMessage = "Contact damage must be between 0 and 500")]
        public int ContactDamage { get; set; }

        public List<BossStageEnemyEditViewModel> SpawnedEnemies { get; set; } = new();
        public List<BossStageDropEditViewModel> Drops { get; set; } = new();
    }

    public class BossStageEnemyEditViewModel
    {
        public string EnemyId { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
        public int Quantity { get; set; }
    }

    public class BossStageDropEditViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
        public int Quantity { get; set; }
    }
}
