using Microsoft.AspNetCore.Mvc.Rendering;

namespace TerrariaDB.ViewModels.Terraria.Recipe
{
    public class RecipeCreateViewModel
    {
        public string ResultItemId { get; set; } = string.Empty;
        public short ResultItemQuantity { get; set; }
        public string? CraftingStationName { get; set; }
        public List<RecipeCreateIngredientViewModel> Ingredients { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
        public List<SelectListItem> AvailableCraftingStations { get; set; } = new();
    }

    public class RecipeCreateIngredientViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
