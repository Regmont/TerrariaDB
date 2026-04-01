namespace TerrariaCompendium.Models.Terraria
{
    public class Boss
    {
        public int BossId { get; set; }
        public int? SummonItemId { get; set; }
        public string BossName { get; set; } = string.Empty;
        public string BossSprite { get; set; } = string.Empty;

        public Item? SummonItem { get; set; }
        public ICollection<BossDrop> BossDrops { get; set; } = new List<BossDrop>();
        public ICollection<BossPart> BossParts { get; set; } = new List<BossPart>();
    }
}
