namespace TerrariaCompendium.Models.Terraria
{
    public class ItemForm
    {
        public int ItemFormId { get; set; }
        public int ItemId { get; set; }
        public string ItemSprite { get; set; } = string.Empty;
        public short ItemFormOrderId { get; set; }
        public string Tooltip { get; set; } = string.Empty;

        public Item Item { get; set; } = null!;
    }
}
