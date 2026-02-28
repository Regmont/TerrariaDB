using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.Item
{
    public class ItemEditViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Item name is required")]
        [StringLength(50, ErrorMessage = "Item name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base price is required")]
        [Range(0, 500000000, ErrorMessage = "Base price must be between 0 and 500,000,000")]
        public int BasePrice { get; set; }

        [Required(ErrorMessage = "Currency type is required")]
        public string CurrencyName { get; set; } = string.Empty;

        public string? CraftingStationName { get; set; }

        [ValidateStages(ErrorMessage = "At least one stage must be selected")]
        public List<string> StageItemIds { get; set; } = new();

        public List<SelectListItem> AvailableCurrencies { get; set; } = new();
        public List<SelectListItem> AvailableCraftingStations { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
    }
}
