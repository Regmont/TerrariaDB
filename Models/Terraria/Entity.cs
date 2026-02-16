namespace TerrariaDB.Models.Terraria
{
    public class Entity
    {
        public short EntityId { get; set; }
        public string GameObjectName { get; set; } = string.Empty;
        public int? Hp { get; set; }
        public short Defense { get; set; }

        public GameObject GameObject { get; set; } = null!;
        public HostileEntity? HostileEntity { get; set; }
        public TownNpc? TownNpc { get; set; }
        public ICollection<EntityDrop> EntityDrops { get; set; } = new List<EntityDrop>();
    }
}
