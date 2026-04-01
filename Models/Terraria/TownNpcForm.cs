namespace TerrariaCompendium.Models.Terraria
{
    public class TownNpcForm
    {
        public int TownNpcFormId { get; set; }
        public int TownNpcId { get; set; }
        public string TownNpcFormSprite { get; set; } = string.Empty;
        public short TownNpcFormOrderId { get; set; }

        public TownNpc TownNpc { get; set; } = null!;
    }
}
