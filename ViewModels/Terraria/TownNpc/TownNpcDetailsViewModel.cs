namespace TerrariaDB.ViewModels.Terraria.TownNpc
{
    public class TownNpcDetailsViewModel
    {
        public string TownNpcId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Defense { get; set; }
        public List<TownNpcDropViewModel> Drops { get; set; } = new();
        public List<TownNpcTradeViewModel> Trades { get; set; } = new();
        public List<TownNpcTransformationViewModel> Transformations { get; set; } = new();
    }

    public class TownNpcDropViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class TownNpcTradeViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
        public string TradeType { get; set; } = string.Empty;
    }

    public class TownNpcTransformationViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Defense { get; set; }
    }
}
