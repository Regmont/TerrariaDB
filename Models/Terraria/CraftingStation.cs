namespace TerrariaDB.Models.Terraria
{
    public class CraftingStation
    {
        public string CraftingStationName { get; set; } = string.Empty;

        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}
