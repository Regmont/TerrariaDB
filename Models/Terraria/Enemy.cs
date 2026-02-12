namespace TerrariaDB.Models.Terraria
{
    public class Enemy
    {
        public short EnemyId { get; set; }
        public short HostileEntityId { get; set; }

        public HostileEntity? HostileEntity { get; set; }
        public ICollection<BossPartEnemies> BossPartEnemies { get; set; } = new List<BossPartEnemies>();
    }
}
