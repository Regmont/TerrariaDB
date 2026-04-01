using TerrariaCompendium.Models.Enums;

namespace TerrariaCompendium.Models.Terraria
{
    public class Item
    {
        public int ItemId { get; set; }
        public int? CraftingStationId { get; set; }
        public short InternalItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public Currency BuyPriceCurrency { get; set; }

        public CraftingStation? CraftingStation { get; set; } = null!;
        public Boss? SummonedBoss { get; set; }
        public ICollection<BossDrop> BossDrops { get; set; } = new List<BossDrop>();
        public ICollection<EntityDrop> EntityDrops { get; set; } = new List<EntityDrop>();
        public ICollection<TradeOffer> TradeOffers { get; set; } = new List<TradeOffer>();
        public ICollection<ItemForm> ItemForms { get; set; } = new List<ItemForm>();
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ICollection<RecipeItems> RecipeItems { get; set; } = new List<RecipeItems>();
    }
}
