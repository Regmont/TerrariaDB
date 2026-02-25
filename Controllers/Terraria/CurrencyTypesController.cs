using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;
using TerrariaDB.ViewModels.Terraria.CurrencyType;

namespace TerrariaDB.Controllers.Terraria
{
    public class CurrencyTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CurrencyTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CurrencyTypes
        [Authorize(Roles = "Admin")]
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
            return View();
        }

        // POST: CurrencyTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CurrencyName")] CurrencyType currencyType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(currencyType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(currencyType);
        }

        // GET: CurrencyTypes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currencyType = await _context.CurrencyType.FindAsync(id);
            if (currencyType == null)
            {
                return NotFound();
            }
            return View(currencyType);
        }

        // POST: CurrencyTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CurrencyName")] CurrencyType currencyType)
        {
            if (id != currencyType.CurrencyName)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(currencyType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CurrencyTypeExists(currencyType.CurrencyName))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(currencyType);
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
