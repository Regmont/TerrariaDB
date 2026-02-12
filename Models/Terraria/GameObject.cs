namespace TerrariaDB.Models.Terraria
{
    public class GameObject
    {
        public string GameObjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Sprite { get; set; } = string.Empty;
        public string? TransformName { get; set; }

        public GameObject? Transform { get; set; }
        public GameObject? TransformedFrom { get; set; }
        public Entity? Entity { get; set; }
        public Item? Item { get; set; }
    }
}
