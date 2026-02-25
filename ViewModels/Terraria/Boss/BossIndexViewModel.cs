namespace TerrariaDB.ViewModels.Terraria.Boss
{
    public class BossIndexViewModel
    {
        public List<BossItemViewModel> Bosses { get; set; } = new();
    }

    public class BossItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }
}
