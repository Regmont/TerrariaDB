using Microsoft.AspNetCore.Mvc;
using TerrariaDB.ViewModels;

namespace TerrariaDB.Controllers
{
    public class BaseController : Controller
    {
        protected bool CanEdit => User.IsInRole("Admin");
        protected bool CanDelete => User.IsInRole("Admin");

        protected T SetPermissions<T>(T viewModel) where T : ViewModelBase
        {
            viewModel.CanEdit = CanEdit;
            viewModel.CanDelete = CanDelete;

            return viewModel;
        }
    }
}
