namespace TerrariaCompendium.Models.Terraria
{
    public class BossDrop
    {
        public int BossDropId { get; set; }
        public int BossId { get; set; }
        public int ItemId { get; set; }
        public short Quantity { get; set; }

        public Boss Boss { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
