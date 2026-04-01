namespace TerrariaCompendium.Models.Terraria
{
    public class BossPartStage
    {
        public int BossPartStageId { get; set; }
        public int BossPartId { get; set; }
        public string BossPartStageSprite { get; set; } = string.Empty;
        public short BossPartStageOrderId { get; set; }
        public short ContactDamage { get; set; }
        public short Hp { get; set; }
        public short Defense { get; set; }

        public BossPart BossPart { get; set; } = null!;
        public ICollection<BossPartStageEnemies> BossPartStageEnemies { get; set; } = new List<BossPartStageEnemies>();
    }
}
