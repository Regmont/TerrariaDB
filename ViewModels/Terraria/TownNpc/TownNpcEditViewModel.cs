using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.TownNpc
{
    public class TownNpcEditViewModel
    {
        public string TownNpcId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Town NPC name is required")]
        [StringLength(50, ErrorMessage = "Town NPC name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        public List<TownNpcEditStageViewModel> Stages { get; set; } = new();
        public List<TownNpcDropEditViewModel> Drops { get; set; } = new();
        public List<TownNpcTradeEditViewModel> Trades { get; set; } = new();
        public List<SelectListItem> AvailableItems { get; set; } = new();
        public List<SelectListItem> AvailableTradeTypes { get; set; } = new();
    }

    public class TownNpcEditStageViewModel
    {
        [Required(ErrorMessage = "Sprite is required")]
        [StringLength(500, ErrorMessage = "Sprite path cannot exceed 500 characters")]
        public string Sprite { get; set; } = string.Empty;

        [Range(0, 30000, ErrorMessage = "HP must be between 0 and 30,000")]
        public int Hp { get; set; }

        [Range(0, 10000, ErrorMessage = "Defense must be between 0 and 10,000")]
        public int Defense { get; set; }

        [Range(-500, 1000, ErrorMessage = "Entity ID must be between -500 and 1000")]
        public short EntityId { get; set; }
    }

    public class TownNpcDropEditViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }

    public class TownNpcTradeEditViewModel
    {
        public string ItemId { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Trade type is required")]
        public string TradeType { get; set; } = string.Empty;
    }
}
