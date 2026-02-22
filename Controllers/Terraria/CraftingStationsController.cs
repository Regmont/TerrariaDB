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
    public class CraftingStationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CraftingStationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CraftingStations
        public async Task<IActionResult> Index()
        {
            return View(await _context.CraftingStation.ToListAsync());
        }

        // GET: CraftingStations/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var craftingStation = await _context.CraftingStation
                .FirstOrDefaultAsync(m => m.CraftingStationName == id);
            if (craftingStation == null)
            {
                return NotFound();
            }

            return View(craftingStation);
        }

        // GET: CraftingStations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CraftingStations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CraftingStationName")] CraftingStation craftingStation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(craftingStation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(craftingStation);
        }

        // GET: CraftingStations/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var craftingStation = await _context.CraftingStation.FindAsync(id);
            if (craftingStation == null)
            {
                return NotFound();
            }
            return View(craftingStation);
        }

        // POST: CraftingStations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CraftingStationName")] CraftingStation craftingStation)
        {
            if (id != craftingStation.CraftingStationName)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(craftingStation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CraftingStationExists(craftingStation.CraftingStationName))
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
            return View(craftingStation);
        }

        // GET: CraftingStations/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var craftingStation = await _context.CraftingStation
                .FirstOrDefaultAsync(m => m.CraftingStationName == id);
            if (craftingStation == null)
            {
                return NotFound();
            }

            return View(craftingStation);
        }

        // POST: CraftingStations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var craftingStation = await _context.CraftingStation.FindAsync(id);
            if (craftingStation != null)
            {
                _context.CraftingStation.Remove(craftingStation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CraftingStationExists(string id)
        {
            return _context.CraftingStation.Any(e => e.CraftingStationName == id);
        }
    }
}
