using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;

namespace TerrariaDB.Controllers.Terraria
{
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
            return View(await _context.TradeType.ToListAsync());
        }

        // GET: TradeTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TradeTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TradeTypeName")] TradeType tradeType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tradeType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tradeType);
        }

        // GET: TradeTypes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tradeType = await _context.TradeType.FindAsync(id);
            if (tradeType == null)
            {
                return NotFound();
            }
            return View(tradeType);
        }

        // POST: TradeTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("TradeTypeName")] TradeType tradeType)
        {
            if (id != tradeType.TradeTypeName)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tradeType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TradeTypeExists(tradeType.TradeTypeName))
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
            return View(tradeType);
        }

        // GET: TradeTypes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tradeType = await _context.TradeType
                .FirstOrDefaultAsync(m => m.TradeTypeName == id);
            if (tradeType == null)
            {
                return NotFound();
            }

            return View(tradeType);
        }

        // POST: TradeTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var tradeType = await _context.TradeType.FindAsync(id);
            if (tradeType != null)
            {
                _context.TradeType.Remove(tradeType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TradeTypeExists(string id)
        {
            return _context.TradeType.Any(e => e.TradeTypeName == id);
        }
    }
}
