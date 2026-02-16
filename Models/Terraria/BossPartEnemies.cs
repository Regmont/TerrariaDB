namespace TerrariaDB.Models.Terraria
{
    public class BossPartEnemies
    {
        public short BossPartId { get; set; }
        public short EnemyId { get; set; }
        public short Quantity { get; set; }

        public BossPart BossPart { get; set; } = null!;
        public Enemy Enemy { get; set; } = null!;
    }
}
