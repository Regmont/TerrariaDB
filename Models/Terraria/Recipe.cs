namespace TerrariaCompendium.Models.Terraria
{
    public class Recipe
    {
        public int RecipeId { get; set; }
        public int ResultItemId { get; set; }
        public int? CraftingStationId { get; set; }
        public short ResultItemQuantity { get; set; }

        public Item ResultItem { get; set; } = null!;
        public CraftingStation? CraftingStation { get; set; } = null!;
        public ICollection<RecipeItems> RecipeItems { get; set; } = new List<RecipeItems>();
    }
}
