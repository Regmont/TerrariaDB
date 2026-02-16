namespace TerrariaDB.Models.Terraria
{
    public class EntityDrop
    {
        public short EntityId { get; set; }
        public short ItemId { get; set; }
        public short Quantity { get; set; }

        public Entity Entity { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
