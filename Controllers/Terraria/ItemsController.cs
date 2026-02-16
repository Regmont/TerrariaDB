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
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Items
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Item.Include(i => i.CraftingStation).Include(i => i.CurrencyType).Include(i => i.GameObject);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Items/Details/5
        public async Task<IActionResult> Details(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item
                .Include(i => i.CraftingStation)
                .Include(i => i.CurrencyType)
                .Include(i => i.GameObject)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Items/Create
        public IActionResult Create()
        {
            ViewData["CraftingStationName"] = new SelectList(_context.CraftingStation, "CraftingStationName", "CraftingStationName");
            ViewData["CurrencyName"] = new SelectList(_context.CurrencyType, "CurrencyName", "CurrencyName");
            ViewData["GameObjectName"] = new SelectList(_context.GameObject, "GameObjectName", "GameObjectName");
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,GameObjectName,CraftingStationName,BasePrice,CurrencyName")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CraftingStationName"] = new SelectList(_context.CraftingStation, "CraftingStationName", "CraftingStationName", item.CraftingStationName);
            ViewData["CurrencyName"] = new SelectList(_context.CurrencyType, "CurrencyName", "CurrencyName", item.CurrencyName);
            ViewData["GameObjectName"] = new SelectList(_context.GameObject, "GameObjectName", "GameObjectName", item.GameObjectName);
            return View(item);
        }

        // GET: Items/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewData["CraftingStationName"] = new SelectList(_context.CraftingStation, "CraftingStationName", "CraftingStationName", item.CraftingStationName);
            ViewData["CurrencyName"] = new SelectList(_context.CurrencyType, "CurrencyName", "CurrencyName", item.CurrencyName);
            ViewData["GameObjectName"] = new SelectList(_context.GameObject, "GameObjectName", "GameObjectName", item.GameObjectName);
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, [Bind("ItemId,GameObjectName,CraftingStationName,BasePrice,CurrencyName")] Item item)
        {
            if (id != item.ItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.ItemId))
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
            ViewData["CraftingStationName"] = new SelectList(_context.CraftingStation, "CraftingStationName", "CraftingStationName", item.CraftingStationName);
            ViewData["CurrencyName"] = new SelectList(_context.CurrencyType, "CurrencyName", "CurrencyName", item.CurrencyName);
            ViewData["GameObjectName"] = new SelectList(_context.GameObject, "GameObjectName", "GameObjectName", item.GameObjectName);
            return View(item);
        }

        // GET: Items/Delete/5
        public async Task<IActionResult> Delete(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item
                .Include(i => i.CraftingStation)
                .Include(i => i.CurrencyType)
                .Include(i => i.GameObject)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var item = await _context.Item.FindAsync(id);
            if (item != null)
            {
                _context.Item.Remove(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(short id)
        {
            return _context.Item.Any(e => e.ItemId == id);
        }
    }
}
