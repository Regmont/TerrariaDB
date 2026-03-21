using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TerrariaCompendium.Areas.Identity.Data;
using TerrariaCompendium.Areas.Identity.Pages;

namespace TerrariaCompendium.Pages.Admin
{
    public class ManageModel : PageModel
    {
        private readonly UserManager<TerrariaCompendiumUser> _userManager;

        public ManageModel(UserManager<TerrariaCompendiumUser> userManager)
        {
            _userManager = userManager;
        }

        public List<UserViewModel> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            Users = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Login = u.UserName ?? string.Empty,
                Name = u.Name,
                Password = u.PlainPassword
            }).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToPage();
        }
    }
}
