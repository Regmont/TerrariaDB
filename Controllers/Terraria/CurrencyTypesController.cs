using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;
using TerrariaDB.ViewModels.Terraria.CurrencyType;

namespace TerrariaDB.Controllers.Terraria
{
    [Authorize(Roles = "Admin")]
    public class CurrencyTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CurrencyTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CurrencyTypes
        public async Task<IActionResult> Index()
        {
            var currencyNames = await _context.CurrencyType
                .Select(ct => ct.CurrencyName)
                .ToListAsync();

            var viewModel = new CurrencyTypeIndexViewModel
            {
                CurrencyNames = currencyNames
            };

            return View(viewModel);
        }

        // GET: CurrencyTypes/Create
        public IActionResult Create()
        {
            var viewModel = new CurrencyTypeCreateViewModel();
            return View(viewModel);
        }

        // POST: CurrencyTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CurrencyTypeCreateViewModel viewModel)
        {
            if (await _context.CurrencyType.AnyAsync(ct => ct.CurrencyName == viewModel.CurrencyName))
            {
                ModelState.AddModelError("CurrencyName", "A currency type with this name already exists");
                return View(viewModel);
            }

            if (ModelState.IsValid)
            {
                var currencyType = new CurrencyType
                {
                    CurrencyName = viewModel.CurrencyName
                };

                _context.Add(currencyType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: CurrencyTypes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var currencyType = await _context.CurrencyType
                .Include(ct => ct.Items)
                .FirstOrDefaultAsync(ct => ct.CurrencyName == id);

            if (currencyType == null)
            {
                return NotFound();
            }

            var viewModel = new CurrencyTypeDeleteViewModel
            {
                CurrencyName = currencyType.CurrencyName,
                HasRelatedItems = currencyType.Items.Any()
            };

            return View(viewModel);
        }

        // POST: CurrencyTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(CurrencyTypeDeleteViewModel viewModel)
        {
            var currencyType = await _context.CurrencyType
                .Include(ct => ct.Items)
                .FirstOrDefaultAsync(ct => ct.CurrencyName == viewModel.CurrencyName);

            if (currencyType == null)
            {
                return NotFound();
            }

            if (currencyType.Items.Any())
            {
                var errorViewModel = new CurrencyTypeDeleteViewModel
                {
                    CurrencyName = currencyType.CurrencyName,
                    HasRelatedItems = true
                };

                return View(errorViewModel);
            }

            _context.CurrencyType.Remove(currencyType);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
