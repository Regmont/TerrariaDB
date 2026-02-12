namespace TerrariaDB.Models.Terraria
{
    public class TradeOffer
    {
        public byte TownNpcId { get; set; }
        public short ItemId { get; set; }
        public string TradeTypeName { get; set; } = string.Empty;
        public short Quantity { get; set; }

        public TownNpc? TownNpc { get; set; }
        public Item? Item { get; set; }
        public TradeType? TradeType { get; set; }
    }
}
