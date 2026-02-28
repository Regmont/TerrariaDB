using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.TradeType
{
    public class TradeTypeCreateViewModel
    {
        [Required(ErrorMessage = "Trade type name is required")]
        [StringLength(50, ErrorMessage = "Trade type name cannot exceed 50 characters")]
        public string TradeTypeName { get; set; } = string.Empty;
    }
}
