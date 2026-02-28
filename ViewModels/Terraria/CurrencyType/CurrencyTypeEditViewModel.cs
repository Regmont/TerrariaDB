using System.ComponentModel.DataAnnotations;

namespace TerrariaDB.ViewModels.Terraria.CurrencyType
{
    public class CurrencyTypeEditViewModel
    {
        public string OriginalCurrencyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Currency name is required")]
        [StringLength(50, ErrorMessage = "Currency type name cannot exceed 50 characters")]
        public string CurrencyName { get; set; } = string.Empty;
    }
}
