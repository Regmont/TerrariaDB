namespace TerrariaDB.ViewModels.Terraria.Item
{
    public class ItemDeleteViewModel
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public bool HasRelatedRecipes { get; set; }
        public bool IsLastCraftingStationItem { get; set; }
    }
}
