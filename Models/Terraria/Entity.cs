namespace TerrariaCompendium.Models.Terraria
{
    public class Entity
    {
        public int EntityId { get; set; }
        public short InternalNpcId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<BossPart> BossParts { get; set; } = new List<BossPart>();
        public ICollection<Enemy> Enemies { get; set; } = new List<Enemy>();
        public ICollection<TownNpc> TownNpcs { get; set; } = new List<TownNpc>();
        public ICollection<EntityDrop> EntityDrops { get; set; } = new List<EntityDrop>();
    }
}
