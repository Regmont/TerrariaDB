namespace TerrariaDB.Models.Terraria
{
    public class Boss
    {
        public string BossName { get; set; } = string.Empty;
        public short? SummonItemId { get; set; }

        public Item? SummonItem { get; set; }
        public ICollection<BossPart> BossParts { get; set; } = new List<BossPart>();
        public ICollection<BossDrop> BossDrops { get; set; } = new List<BossDrop>();
    }
}
