namespace TerrariaDB.Models.Terraria
{
    public class BossDrop
    {
        public string BossName { get; set; } = string.Empty;
        public short ItemId { get; set; }
        public short Quantity { get; set; }

        public Boss Boss { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
