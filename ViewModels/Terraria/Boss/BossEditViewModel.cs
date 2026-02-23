using Microsoft.AspNetCore.Mvc.Rendering;

namespace TerrariaDB.ViewModels.Terraria.Boss
{
    public class BossEditViewModel
    {
        public string OriginalBossName { get; set; } = string.Empty;
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
        public int Quantity { get; set; }
    }

    public class BossPartEditViewModel
    {
        public string PartName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public List<BossStageEditViewModel> Stages { get; set; } = new();
    }

    public class BossStageEditViewModel
    {
        public string Sprite { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Defense { get; set; }
        public int EntityId { get; set; }
        public int ContactDamage { get; set; }
        public List<BossStageEnemyEditViewModel> SpawnedEnemies { get; set; } = new();
        public List<BossStageDropEditViewModel> Drops { get; set; } = new();
    }

    public class BossStageEnemyEditViewModel
    {
        public string EnemyId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class BossStageDropEditViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
