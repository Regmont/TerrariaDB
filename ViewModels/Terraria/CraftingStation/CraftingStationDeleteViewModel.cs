namespace TerrariaDB.ViewModels.Terraria.CraftingStation
{
    public class CraftingStationDeleteViewModel
    {
        public string CraftingStationName { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
        public bool HasRelatedRecipes { get; set; }
    }
}
