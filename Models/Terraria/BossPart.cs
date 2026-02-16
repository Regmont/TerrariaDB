namespace TerrariaDB.Models.Terraria
{
    public class BossPart
    {
        public short BossPartId { get; set; }
        public string BossName { get; set; } = string.Empty;
        public short HostileEntityId { get; set; }
        public short Quantity { get; set; }

        public Boss Boss { get; set; } = null!;
        public HostileEntity HostileEntity { get; set; } = null!;
        public ICollection<BossPartEnemies> BossPartEnemies { get; set; } = new List<BossPartEnemies>();
    }
}
