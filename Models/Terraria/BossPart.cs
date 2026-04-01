namespace TerrariaCompendium.Models.Terraria
{
    public class BossPart
    {
        public int BossPartId { get; set; }
        public int BossId { get; set; }
        public int EntityId { get; set; }
        public short BossPartOrderId { get; set; }
        public short Quantity { get; set; }

        public Boss Boss { get; set; } = null!;
        public Entity Entity { get; set; } = null!;
        public ICollection<BossPartStage> BossPartStages { get; set; } = new List<BossPartStage>();
    }
}
