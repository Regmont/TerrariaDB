using Microsoft.AspNetCore.Mvc.Rendering;

namespace TerrariaDB.ViewModels.Terraria.Recipe
{
    public class RecipeIndexViewModel
    {
        public List<RecipeItemViewModel> Recipes { get; set; } = new();
        public List<SelectListItem> ResultItemFilterOptions { get; set; } = new();
        public List<SelectListItem> IngredientFilterOptions { get; set; } = new();
        public List<SelectListItem> CraftingStationFilterOptions { get; set; } = new();
    }

    public class RecipeItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public RecipeItemInfoViewModel ResultItem { get; set; } = new();
        public int ResultItemQuantity { get; set; }
        public List<RecipeIngredientViewModel> Ingredients { get; set; } = new();
        public RecipeStationViewModel? CraftingStation { get; set; }
    }

    public class RecipeItemInfoViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }

    public class RecipeIngredientViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class RecipeStationViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }
}
