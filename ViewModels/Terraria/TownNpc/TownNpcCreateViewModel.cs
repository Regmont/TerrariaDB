using Microsoft.AspNetCore.Mvc.Rendering;

namespace TerrariaDB.ViewModels.Terraria.TownNpc
{
    public class TownNpcCreateViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<TownNpcCreateStageViewModel> Stages { get; set; } = new();
        public List<TownNpcDropCreateViewModel> Drops { get; set; } = new();
        public List<TownNpcTradeCreateViewModel> Trades { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
        public List<SelectListItem> AvailableTradeTypes { get; set; } = new();
    }

    public class TownNpcCreateStageViewModel
    {
        public string Sprite { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Defense { get; set; }
        public int EntityId { get; set; }
    }

    public class TownNpcDropCreateViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class TownNpcTradeCreateViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string TradeType { get; set; } = string.Empty;
    }
}
