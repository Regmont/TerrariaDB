namespace TerrariaDB.ViewModels.Terraria.Item
{
    public class ItemIndexViewModel
    {
        public List<ItemItemViewModel> Items { get; set; } = new();
    }

    public class ItemItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }
}
