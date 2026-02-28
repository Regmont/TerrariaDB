using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.Recipe
{
    public class RecipeEditViewModel
    {
        public string RecipeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Result item is required")]
        public string ResultItemId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Result quantity is required")]
        [Range(1, 100, ErrorMessage = "Result quantity must be between 1 and 100")]
        public short ResultItemQuantity { get; set; }

        public string? CraftingStationName { get; set; }

        [ValidateIngredients(ErrorMessage = "At least one ingredient is required")]
        public List<RecipeEditIngredientViewModel> Ingredients { get; set; } = new();

        public List<SelectListItem> AvailableItems { get; set; } = new();
        public List<SelectListItem> AvailableCraftingStations { get; set; } = new();
    }

    public class RecipeEditIngredientViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000")]
        public int Quantity { get; set; }
    }
}
