namespace TerrariaDB.ViewModels.Terraria.Enemy
{
    public class EnemyDetailsViewModel
    {
        public string EnemyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Defense { get; set; }
        public int ContactDamage { get; set; }
        public List<EnemyDropViewModel> Drops { get; set; } = new();
        public List<EnemyTransformationViewModel> Transformations { get; set; } = new();
    }

    public class EnemyDropViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class EnemyTransformationViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Defense { get; set; }
        public int ContactDamage { get; set; }
        public List<EnemyDropViewModel> Drops { get; set; } = new();
    }
}
