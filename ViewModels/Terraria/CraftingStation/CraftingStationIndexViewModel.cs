namespace TerrariaDB.ViewModels.Terraria.CraftingStation
{
    public class CraftingStationIndexViewModel
    {
        public List<CraftingStationItemViewModel> CraftingStations { get; set; } = new();
    }

    public class CraftingStationItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }
}
