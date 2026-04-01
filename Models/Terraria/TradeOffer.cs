using TerrariaCompendium.Models.Enums;

namespace TerrariaCompendium.Models.Terraria
{
    public class TradeOffer
    {
        public int TradeOfferId { get; set; }
        public int TownNpcId { get; set; }
        public int ItemId { get; set; }
        public short Quantity { get; set; }
        public TradeType TradeType { get; set; }

        public TownNpc TownNpc { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
