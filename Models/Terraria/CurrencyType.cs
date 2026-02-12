namespace TerrariaDB.Models.Terraria
{
    public class CurrencyType
    {
        public string CurrencyName { get; set; } = string.Empty;

        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
