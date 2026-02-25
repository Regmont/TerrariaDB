namespace TerrariaDB.ViewModels.Terraria.CraftingStation
{
    public class CraftingStationDetailsViewModel
    {
        public string CraftingStationName { get; set; } = string.Empty;
        public List<CraftingStationItemDetailsViewModel> Items { get; set; } = new();
    }

    public class CraftingStationItemDetailsViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }
}
