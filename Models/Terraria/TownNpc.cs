namespace TerrariaDB.Models.Terraria
{
    public class TownNpc
    {
        public byte TownNpcId { get; set; }
        public short EntityId { get; set; }

        public Entity Entity { get; set; } = null!;
        public ICollection<TradeOffer> TradeOffers { get; set; } = new List<TradeOffer>();
    }
}
