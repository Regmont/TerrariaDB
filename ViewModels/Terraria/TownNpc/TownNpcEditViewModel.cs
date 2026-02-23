using Microsoft.AspNetCore.Mvc.Rendering;

namespace TerrariaDB.ViewModels.Terraria.TownNpc
{
    public class TownNpcEditViewModel
    {
        public string TownNpcId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<TownNpcEditStageViewModel> Stages { get; set; } = new();
        public List<TownNpcDropEditViewModel> Drops { get; set; } = new();
        public List<TownNpcTradeEditViewModel> Trades { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
        public List<SelectListItem> AvailableTradeTypes { get; set; } = new();
    }

    public class TownNpcEditStageViewModel
    {
        public string Sprite { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Defense { get; set; }
        public int EntityId { get; set; }
    }

    public class TownNpcDropEditViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class TownNpcTradeEditViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string TradeType { get; set; } = string.Empty;
    }
}
