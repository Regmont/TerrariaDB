using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.CraftingStation
{
    public class CraftingStationEditViewModel
    {
        public string OriginalCraftingStationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Crafting station name is required")]
        [StringLength(50, ErrorMessage = "Crafting station name cannot exceed 50 characters")]
        public string CraftingStationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an item")]
        public string SelectedItemId { get; set; } = string.Empty;

        public List<SelectListItem> AvailableItems { get; set; } = new();
    }
}
