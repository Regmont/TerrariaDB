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
    public class EnemiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnemiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Enemies
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Enemy.Include(e => e.HostileEntity);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Enemies/Details/5
        public async Task<IActionResult> Details(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enemy = await _context.Enemy
                .Include(e => e.HostileEntity)
                .FirstOrDefaultAsync(m => m.EnemyId == id);
            if (enemy == null)
            {
                return NotFound();
            }

            return View(enemy);
        }

        // GET: Enemies/Create
        public IActionResult Create()
        {
            ViewData["HostileEntityId"] = new SelectList(_context.HostileEntity, "HostileEntityId", "HostileEntityId");
            return View();
        }

        // POST: Enemies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnemyId,HostileEntityId")] Enemy enemy)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enemy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["HostileEntityId"] = new SelectList(_context.HostileEntity, "HostileEntityId", "HostileEntityId", enemy.HostileEntityId);
            return View(enemy);
        }

        // GET: Enemies/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enemy = await _context.Enemy.FindAsync(id);
            if (enemy == null)
            {
                return NotFound();
            }
            ViewData["HostileEntityId"] = new SelectList(_context.HostileEntity, "HostileEntityId", "HostileEntityId", enemy.HostileEntityId);
            return View(enemy);
        }

        // POST: Enemies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, [Bind("EnemyId,HostileEntityId")] Enemy enemy)
        {
            if (id != enemy.EnemyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enemy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnemyExists(enemy.EnemyId))
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
            ViewData["HostileEntityId"] = new SelectList(_context.HostileEntity, "HostileEntityId", "HostileEntityId", enemy.HostileEntityId);
            return View(enemy);
        }

        // GET: Enemies/Delete/5
        public async Task<IActionResult> Delete(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enemy = await _context.Enemy
                .Include(e => e.HostileEntity)
                .FirstOrDefaultAsync(m => m.EnemyId == id);
            if (enemy == null)
            {
                return NotFound();
            }

            return View(enemy);
        }

        // POST: Enemies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var enemy = await _context.Enemy.FindAsync(id);
            if (enemy != null)
            {
                _context.Enemy.Remove(enemy);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnemyExists(short id)
        {
            return _context.Enemy.Any(e => e.EnemyId == id);
        }
    }
}
