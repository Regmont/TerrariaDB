namespace TerrariaDB.Models.Terraria
{
    public class Recipe
    {
        public short RecipeId { get; set; }
        public short ResultItemId { get; set; }
        public string? CraftingStationName { get; set; }
        public short ResultItemQuantity { get; set; }

        public Item ResultItem { get; set; } = null!;
        public CraftingStation? CraftingStation { get; set; }
        public ICollection<RecipeItems> RecipeItems { get; set; } = new List<RecipeItems>();
    }
}
