using Microsoft.AspNetCore.Mvc.Rendering;

namespace TerrariaDB.ViewModels.Terraria.Item
{
    public class ItemEditViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int BasePrice { get; set; }
        public string CurrencyName { get; set; } = string.Empty;
        public string? CraftingStationName { get; set; }
        public List<string> StageItemIds { get; set; } = new();
        public List<SelectListItem> AvailableCurrencies { get; set; } = new();
        public List<SelectListItem> AvailableCraftingStations { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
    }
}
