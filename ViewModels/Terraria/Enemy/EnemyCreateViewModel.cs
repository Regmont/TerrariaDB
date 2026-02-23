using Microsoft.AspNetCore.Mvc.Rendering;

namespace TerrariaDB.ViewModels.Terraria.Enemy
{
    public class EnemyCreateViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<EnemyStageViewModel> Stages { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
    }

    public class EnemyStageViewModel
    {
        public string Sprite { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Defense { get; set; }
        public int EntityId { get; set; }
        public int ContactDamage { get; set; }
        public List<EnemyDropCreateViewModel> Drops { get; set; } = new();
    }

    public class EnemyDropCreateViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
