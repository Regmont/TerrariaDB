namespace TerrariaCompendium.Models.Terraria
{
    public class Enemy
    {
        public int EnemyId { get; set; }
        public int EntityId { get; set; }
        public short ContactDamage { get; set; }
        public short Hp { get; set; }
        public short Defense { get; set; }

        public Entity Entity { get; set; } = null!;
        public ICollection<BossPartStageEnemies> BossPartStageEnemies { get; set; } = new List<BossPartStageEnemies>();
    }
}
