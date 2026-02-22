namespace TerrariaDB.ViewModels.Terraria.TownNpc
{
    public class TownNpcIndexViewModel
    {
        public List<TownNpcItemViewModel> TownNpcs { get; set; } = new();
    }

    public class TownNpcItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }
}
