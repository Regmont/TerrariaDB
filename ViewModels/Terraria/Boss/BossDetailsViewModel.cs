namespace TerrariaDB.ViewModels.Terraria.Boss
{
    public class BossDetailsViewModel
    {
        public string BossName { get; set; } = string.Empty;
        public List<BossDropViewModel> Drops { get; set; } = new();
        public List<BossPartViewModel> BossParts { get; set; } = new();
    }

    public class BossDropViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class BossPartViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public List<BossStageViewModel> Stages { get; set; } = new();
    }

    public class BossStageViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Defense { get; set; }
        public int ContactDamage { get; set; }
        public List<BossStageEnemyViewModel> SummonedEnemies { get; set; } = new();
        public List<BossStageDropViewModel> Drops { get; set; } = new();
    }

    public class BossStageEnemyViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class BossStageDropViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
