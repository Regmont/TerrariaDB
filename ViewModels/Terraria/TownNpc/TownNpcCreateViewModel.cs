using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.TownNpc
{
    public class TownNpcCreateViewModel
    {
        [Required(ErrorMessage = "Town NPC name is required")]
        [StringLength(50, ErrorMessage = "Town NPC name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string? Description { get; set; }

        public List<TownNpcCreateStageViewModel> Stages { get; set; } = new();
        public List<TownNpcDropCreateViewModel> Drops { get; set; } = new();
        public List<TownNpcTradeCreateViewModel> Trades { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
        public List<SelectListItem> AvailableTradeTypes { get; set; } = new();
    }

    public class TownNpcCreateStageViewModel
    {
        [Required(ErrorMessage = "Sprite is required")]
        [StringLength(500, ErrorMessage = "Sprite path cannot exceed 500 characters")]
        public string Sprite { get; set; } = string.Empty;

        public int Hp { get; set; }

        public int Defense { get; set; }

        [Range(-500, 1000, ErrorMessage = "Entity ID must be between -500 and 1000")]
        public short EntityId { get; set; }
    }

    public class TownNpcDropCreateViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }

    public class TownNpcTradeCreateViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        public int Quantity { get; set; }

        [Required(ErrorMessage = "Trade type is required")]
        public string TradeType { get; set; } = string.Empty;
    }
}
