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
    public class BossesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BossesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bosses
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Boss.Include(b => b.SummonItem);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Bosses/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boss = await _context.Boss
                .Include(b => b.SummonItem)
                .FirstOrDefaultAsync(m => m.BossName == id);
            if (boss == null)
            {
                return NotFound();
            }

            return View(boss);
        }

        // GET: Bosses/Create
        public IActionResult Create()
        {
            ViewData["SummonItemId"] = new SelectList(_context.Item, "ItemId", "CurrencyName");
            return View();
        }

        // POST: Bosses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BossName,SummonItemId")] Boss boss)
        {
            if (ModelState.IsValid)
            {
                _context.Add(boss);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SummonItemId"] = new SelectList(_context.Item, "ItemId", "CurrencyName", boss.SummonItemId);
            return View(boss);
        }

        // GET: Bosses/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boss = await _context.Boss.FindAsync(id);
            if (boss == null)
            {
                return NotFound();
            }
            ViewData["SummonItemId"] = new SelectList(_context.Item, "ItemId", "CurrencyName", boss.SummonItemId);
            return View(boss);
        }

        // POST: Bosses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("BossName,SummonItemId")] Boss boss)
        {
            if (id != boss.BossName)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(boss);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BossExists(boss.BossName))
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
            ViewData["SummonItemId"] = new SelectList(_context.Item, "ItemId", "CurrencyName", boss.SummonItemId);
            return View(boss);
        }

        // GET: Bosses/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boss = await _context.Boss
                .Include(b => b.SummonItem)
                .FirstOrDefaultAsync(m => m.BossName == id);
            if (boss == null)
            {
                return NotFound();
            }

            return View(boss);
        }

        // POST: Bosses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var boss = await _context.Boss.FindAsync(id);
            if (boss != null)
            {
                _context.Boss.Remove(boss);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BossExists(string id)
        {
            return _context.Boss.Any(e => e.BossName == id);
        }
    }
}
