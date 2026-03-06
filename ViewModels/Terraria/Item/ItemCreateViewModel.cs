using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.Item
{
    public class ItemCreateViewModel
    {
        [Required(ErrorMessage = "First item ID is required")]
        public short FirstItemId { get; set; }

        public short? SecondItemId { get; set; }
        public short? ThirdItemId { get; set; }
        public short? FourthItemId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [StringLength(50, ErrorMessage = "Item name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string? Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base price is required")]
        public int BasePrice { get; set; }

        [Required(ErrorMessage = "Currency type is required")]
        public string CurrencyName { get; set; } = string.Empty;

        public string? CraftingStationName { get; set; }

        public List<StageSpriteViewModel> Stages { get; set; } = new();

        public List<SelectListItem> AvailableCurrencies { get; set; } = new();
        public List<SelectListItem> AvailableCraftingStations { get; set; } = new();
    }

    public class StageSpriteViewModel
    {
        [Required(ErrorMessage = "First stage sprite is required")]
        public string Sprite { get; set; } = string.Empty;
    }
}
