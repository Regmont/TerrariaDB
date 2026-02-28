using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;
using TerrariaDB.ViewModels.Terraria.TradeType;

namespace TerrariaDB.Controllers.Terraria
{
    [Authorize(Roles = "Admin")]
    public class TradeTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TradeTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TradeTypes
        public async Task<IActionResult> Index()
        {
            var tradeTypeNames = await _context.TradeType
                .Select(tt => tt.TradeTypeName)
                .ToListAsync();

            var viewModel = new TradeTypeIndexViewModel
            {
                TradeTypeNames = tradeTypeNames
            };

            return View(viewModel);
        }

        // GET: TradeTypes/Create
        public IActionResult Create()
        {
            var viewModel = new TradeTypeCreateViewModel();
            return View(viewModel);
        }

        // POST: TradeTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TradeTypeCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (await _context.TradeType.AnyAsync(tt => tt.TradeTypeName == viewModel.TradeTypeName))
                {
                    ModelState.AddModelError("TradeTypeName", "A trade type with this name already exists");
                    return View(viewModel);
                }

                var tradeType = new TradeType
                {
                    TradeTypeName = viewModel.TradeTypeName
                };

                _context.Add(tradeType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: TradeTypes/Edit/5
        public IActionResult Edit(string tradeTypeName)
        {
            if (!TradeTypeExists(tradeTypeName))
            {
                return NotFound();
            }

            var viewModel = new TradeTypeEditViewModel
            {
                OriginalTradeTypeName = tradeTypeName,
                TradeTypeName = tradeTypeName
            };

            return View(viewModel);
        }

        // POST: TradeTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TradeTypeEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var tradeType = await _context.TradeType
                    .Include(tt => tt.TradeOffers)
                    .FirstOrDefaultAsync(tt => tt.TradeTypeName == viewModel.OriginalTradeTypeName);

                if (tradeType == null)
                {
                    return NotFound();
                }

                if (viewModel.OriginalTradeTypeName != viewModel.TradeTypeName &&
                    await _context.TradeType.AnyAsync(tt => tt.TradeTypeName == viewModel.TradeTypeName))
                {
                    ModelState.AddModelError("TradeTypeName", "A trade type with this name already exists");
                    return View(viewModel);
                }

                foreach (var offer in tradeType.TradeOffers)
                {
                    offer.TradeTypeName = viewModel.TradeTypeName;
                }

                tradeType.TradeTypeName = viewModel.TradeTypeName;

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TradeTypeExists(viewModel.OriginalTradeTypeName))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(viewModel);
        }

        // GET: TradeTypes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var tradeType = await _context.TradeType
                .Include(tt => tt.TradeOffers)
                .FirstOrDefaultAsync(tt => tt.TradeTypeName == id);

            if (tradeType == null)
            {
                return NotFound();
            }

            var viewModel = new TradeTypeDeleteViewModel
            {
                TradeTypeName = tradeType.TradeTypeName,
                HasRelatedTrades = tradeType.TradeOffers.Any()
            };

            return View(viewModel);
        }

        // POST: TradeTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(TradeTypeDeleteViewModel viewModel)
        {
            var tradeType = await _context.TradeType
                .Include(tt => tt.TradeOffers)
                .FirstOrDefaultAsync(tt => tt.TradeTypeName == viewModel.TradeTypeName);

            if (tradeType == null)
            {
                return NotFound();
            }

            if (tradeType.TradeOffers.Any())
            {
                var errorViewModel = new TradeTypeDeleteViewModel
                {
                    TradeTypeName = tradeType.TradeTypeName,
                    HasRelatedTrades = true
                };
                return View(errorViewModel);
            }

            _context.TradeType.Remove(tradeType);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TradeTypeExists(string id)
        {
            return _context.TradeType.Any(e => e.TradeTypeName == id);
        }
    }
}
