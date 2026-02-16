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
    public class TownNpcsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TownNpcsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TownNpcs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TownNpc.Include(t => t.Entity);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TownNpcs/Details/5
        public async Task<IActionResult> Details(byte? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var townNpc = await _context.TownNpc
                .Include(t => t.Entity)
                .FirstOrDefaultAsync(m => m.TownNpcId == id);
            if (townNpc == null)
            {
                return NotFound();
            }

            return View(townNpc);
        }

        // GET: TownNpcs/Create
        public IActionResult Create()
        {
            ViewData["EntityId"] = new SelectList(_context.Entity, "EntityId", "GameObjectName");
            return View();
        }

        // POST: TownNpcs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TownNpcId,EntityId")] TownNpc townNpc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(townNpc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EntityId"] = new SelectList(_context.Entity, "EntityId", "GameObjectName", townNpc.EntityId);
            return View(townNpc);
        }

        // GET: TownNpcs/Edit/5
        public async Task<IActionResult> Edit(byte? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var townNpc = await _context.TownNpc.FindAsync(id);
            if (townNpc == null)
            {
                return NotFound();
            }
            ViewData["EntityId"] = new SelectList(_context.Entity, "EntityId", "GameObjectName", townNpc.EntityId);
            return View(townNpc);
        }

        // POST: TownNpcs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(byte id, [Bind("TownNpcId,EntityId")] TownNpc townNpc)
        {
            if (id != townNpc.TownNpcId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(townNpc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TownNpcExists(townNpc.TownNpcId))
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
            ViewData["EntityId"] = new SelectList(_context.Entity, "EntityId", "GameObjectName", townNpc.EntityId);
            return View(townNpc);
        }

        // GET: TownNpcs/Delete/5
        public async Task<IActionResult> Delete(byte? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var townNpc = await _context.TownNpc
                .Include(t => t.Entity)
                .FirstOrDefaultAsync(m => m.TownNpcId == id);
            if (townNpc == null)
            {
                return NotFound();
            }

            return View(townNpc);
        }

        // POST: TownNpcs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(byte id)
        {
            var townNpc = await _context.TownNpc.FindAsync(id);
            if (townNpc != null)
            {
                _context.TownNpc.Remove(townNpc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TownNpcExists(byte id)
        {
            return _context.TownNpc.Any(e => e.TownNpcId == id);
        }
    }
}
