namespace TerrariaDB.ViewModels.Terraria.Item
{
    public class ItemDetailsViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public int BasePrice { get; set; }
        public string CurrencyType { get; set; } = string.Empty;

        public string? SummonedBossName { get; set; }
        public string? SummonedBossSprite { get; set; }

        public string? CraftingStationName { get; set; }

        public List<ItemTransformationViewModel> Transformations { get; set; } = new();

        public List<ItemBossDropViewModel> DroppedFromBosses { get; set; } = new();
        public List<ItemEntityDropViewModel> DroppedFromEntities { get; set; } = new();

        public List<ItemTradeViewModel> TradedByNpcs { get; set; } = new();
    }

    public class ItemTransformationViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }

    public class ItemBossDropViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }

    public class ItemEntityDropViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }

    public class ItemTradeViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
        public string TradeType { get; set; } = string.Empty;
    }
}
