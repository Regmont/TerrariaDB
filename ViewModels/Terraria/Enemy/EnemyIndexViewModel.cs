namespace TerrariaDB.ViewModels.Terraria.Enemy
{
    public class EnemyIndexViewModel
    {
        public List<EnemyItemViewModel> Enemies { get; set; } = new();
    }

    public class EnemyItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Sprite { get; set; } = string.Empty;
    }
}
