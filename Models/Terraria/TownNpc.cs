namespace TerrariaCompendium.Models.Terraria
{
    public class TownNpc
    {
        public int TownNpcId { get; set; }
        public int EntityId { get; set; }
        public short Hp { get; set; }
        public short Defense { get; set; }

        public Entity Entity { get; set; } = null!;
        public ICollection<TownNpcForm> TownNpcForms { get; set; } = new List<TownNpcForm>();
        public ICollection<TradeOffer> TradeOffers { get; set; } = new List<TradeOffer>();
    }
}
