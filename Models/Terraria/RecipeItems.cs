namespace TerrariaCompendium.Models.Terraria
{
    public class RecipeItems
    {
        public int RecipeItemsId { get; set; }
        public int RecipeId { get; set; }
        public int ItemId { get; set; }
        public short Quantity { get; set; }

        public Recipe Recipe { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
