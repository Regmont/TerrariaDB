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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CurrencyTypeCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (await _context.CurrencyType.AnyAsync(ct => ct.CurrencyName == viewModel.CurrencyName))
                {
                    ModelState.AddModelError("CurrencyName", "A currency type with this name already exists");
                    return View(viewModel);
                }

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

        // GET: CurrencyTypes/Edit/5
        public IActionResult Edit(string currencyName)
        {
            var viewModel = new CurrencyTypeEditViewModel
            {
                OriginalCurrencyName = currencyName,
                CurrencyName = currencyName
            };

            return View(viewModel);
        }

        // POST: CurrencyTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CurrencyTypeEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var currencyType = await _context.CurrencyType.FindAsync(viewModel.OriginalCurrencyName);

                if (currencyType == null)
                {
                    return NotFound();
                }

                if (viewModel.OriginalCurrencyName != viewModel.CurrencyName &&
                    await _context.CurrencyType.AnyAsync(ct => ct.CurrencyName == viewModel.CurrencyName))
                {
                    ModelState.AddModelError("CurrencyName", "A currency type with this name already exists");
                    return View(viewModel);
                }

                currencyType.CurrencyName = viewModel.CurrencyName;

                try
                {
                    _context.Update(currencyType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CurrencyTypeExists(viewModel.OriginalCurrencyName))
                    {
                        return NotFound();
                    }
                    throw;
                }
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
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var currencyType = await _context.CurrencyType
                .Include(ct => ct.Items)
                .FirstOrDefaultAsync(ct => ct.CurrencyName == id);

            if (currencyType == null)
            {
                return NotFound();
            }

            if (currencyType.Items.Any())
            {
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.CurrencyType.Remove(currencyType);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool CurrencyTypeExists(string id)
        {
            return _context.CurrencyType.Any(e => e.CurrencyName == id);
        }
    }
}
