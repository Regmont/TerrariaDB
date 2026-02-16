namespace TerrariaDB.Models.Terraria
{
    public class Item
    {
        public short ItemId { get; set; }
        public string GameObjectName { get; set; } = string.Empty;
        public string? CraftingStationName { get; set; }
        public int BasePrice { get; set; }
        public string CurrencyName { get; set; } = string.Empty;

        public GameObject GameObject { get; set; } = null!;
        public CraftingStation? CraftingStation { get; set; }
        public Boss? SummonedBoss { get; set; }
        public CurrencyType CurrencyType { get; set; } = null!;
        public ICollection<BossDrop> BossDrops { get; set; } = new List<BossDrop>();
        public ICollection<EntityDrop> EntityDrops { get; set; } = new List<EntityDrop>();
        public ICollection<TradeOffer> TradeOffers { get; set; } = new List<TradeOffer>();
        public ICollection<Recipe> ResultRecipes { get; set; } = new List<Recipe>();
        public ICollection<RecipeItems> RecipeItems { get; set; } = new List<RecipeItems>();
    }
}
