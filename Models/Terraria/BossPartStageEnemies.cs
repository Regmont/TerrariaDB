namespace TerrariaCompendium.Models.Terraria
{
    public class BossPartStageEnemies
    {
        public int BossPartStageEnemiesId { get; set; }
        public int BossPartStageId { get; set; }
        public int EnemyId { get; set; }
        public short Quantity { get; set; }

        public BossPartStage BossPartStage { get; set; } = null!;
        public Enemy Enemy { get; set; } = null!;
    }
}
