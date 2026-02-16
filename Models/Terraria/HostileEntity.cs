namespace TerrariaDB.Models.Terraria
{
    public class HostileEntity
    {
        public short HostileEntityId { get; set; }
        public short EntityId { get; set; }
        public short ContactDamage { get; set; }

        public Entity Entity { get; set; } = null!;
        public BossPart? BossPart { get; set; }
        public Enemy? Enemy { get; set; }
    }
}
