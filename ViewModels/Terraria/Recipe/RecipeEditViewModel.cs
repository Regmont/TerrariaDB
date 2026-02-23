using Microsoft.AspNetCore.Mvc.Rendering;

namespace TerrariaDB.ViewModels.Terraria.Recipe
{
    public class RecipeEditViewModel
    {
        public string RecipeId { get; set; } = string.Empty;
        public string ResultItemId { get; set; } = string.Empty;
        public int ResultItemQuantity { get; set; }
        public string? CraftingStationName { get; set; }
        public List<RecipeEditIngredientViewModel> Ingredients { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
        public List<SelectListItem> AvailableCraftingStations { get; set; } = new();
    }

    public class RecipeEditIngredientViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
