using Microsoft.AspNetCore.Mvc.Rendering;

namespace TerrariaDB.ViewModels.Terraria.Boss
{
    public class BossCreateViewModel
    {
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
        public string PartName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public List<BossStageCreateViewModel> Stages { get; set; } = new();
    }

    public class BossStageCreateViewModel
    {
        public string Sprite { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Defense { get; set; }
        public int EntityId { get; set; }
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
