namespace TerrariaDB.Models.Terraria
{
    public class TradeType
    {
        public string TradeTypeName { get; set; } = string.Empty;

        public ICollection<TradeOffer> TradeOffers { get; set; } = new List<TradeOffer>();
    }
}
