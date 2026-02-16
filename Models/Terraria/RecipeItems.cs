namespace TerrariaDB.Models.Terraria
{
    public class RecipeItems
    {
        public short RecipeId { get; set; }
        public short ItemId { get; set; }
        public short Quantity { get; set; }

        public Recipe Recipe { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
