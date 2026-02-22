namespace TerrariaDB.ViewModels.Terraria.Entity
{
    public class EntityIndexViewModel
    {
        public List<EntityItemViewModel> Entities { get; set; } = new();
    }

    public class EntityItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }
}
