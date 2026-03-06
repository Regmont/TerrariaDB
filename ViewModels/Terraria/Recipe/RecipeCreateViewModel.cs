using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.Recipe
{
    public class RecipeCreateViewModel
    {
        [Required(ErrorMessage = "Result item is required")]
        public string ResultItemId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Result quantity is required")]
        public short ResultItemQuantity { get; set; }

        public string? CraftingStationName { get; set; }

        [ValidateIngredients(ErrorMessage = "At least one ingredient is required")]
        public List<RecipeCreateIngredientViewModel> Ingredients { get; set; } = new();

        public List<SelectListItem> AvailableItems { get; set; } = new();
        public List<SelectListItem> AvailableCraftingStations { get; set; } = new();
    }

    public class RecipeCreateIngredientViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }

    public class ValidateIngredientsAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var ingredients = value as List<RecipeCreateIngredientViewModel>;
            if (ingredients == null || !ingredients.Any(i => !string.IsNullOrEmpty(i.ItemId) && i.Quantity > 0))
            {
                return new ValidationResult(ErrorMessage ?? "At least one ingredient is required");
            }
            return ValidationResult.Success;
        }
    }
}
