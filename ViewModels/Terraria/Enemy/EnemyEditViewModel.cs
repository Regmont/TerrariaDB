using Microsoft.AspNetCore.Mvc.Rendering;

namespace TerrariaDB.ViewModels.Terraria.Enemy
{
    public class EnemyEditViewModel
    {
        public string EnemyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<EnemyEditStageViewModel> Stages { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
    }

    public class EnemyEditStageViewModel
    {
        public string Sprite { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Defense { get; set; }
        public int EntityId { get; set; }
        public int ContactDamage { get; set; }
        public List<EnemyDropEditViewModel> Drops { get; set; } = new();
    }

    public class EnemyDropEditViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
