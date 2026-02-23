using Microsoft.AspNetCore.Mvc.Rendering;

namespace TerrariaDB.ViewModels.Terraria.CraftingStation
{
    public class CraftingStationEditViewModel
    {
        public string OriginalCraftingStationName { get; set; } = string.Empty;
        public string CraftingStationName { get; set; } = string.Empty;
        public string SelectedItemId { get; set; } = string.Empty;
        public List<SelectListItem> AvailableItems { get; set; } = new();
    }
}
