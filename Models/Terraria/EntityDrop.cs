namespace TerrariaCompendium.Models.Terraria
{
    public class EntityDrop
    {
        public int EntityDropId { get; set; }
        public int EntityId { get; set; }
        public int ItemId { get; set; }
        public short Quantity { get; set; }

        public Entity Entity { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
