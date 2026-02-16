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
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Recipes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Recipe.Include(r => r.CraftingStation).Include(r => r.ResultItem);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Recipes/Create
        public IActionResult Create()
        {
            ViewData["CraftingStationName"] = new SelectList(_context.CraftingStation, "CraftingStationName", "CraftingStationName");
            ViewData["ResultItemId"] = new SelectList(_context.Item, "ItemId", "CurrencyName");
            return View();
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecipeId,ResultItemId,CraftingStationName,ResultItemQuantity")] Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(recipe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CraftingStationName"] = new SelectList(_context.CraftingStation, "CraftingStationName", "CraftingStationName", recipe.CraftingStationName);
            ViewData["ResultItemId"] = new SelectList(_context.Item, "ItemId", "CurrencyName", recipe.ResultItemId);
            return View(recipe);
        }

        // GET: Recipes/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipe.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            ViewData["CraftingStationName"] = new SelectList(_context.CraftingStation, "CraftingStationName", "CraftingStationName", recipe.CraftingStationName);
            ViewData["ResultItemId"] = new SelectList(_context.Item, "ItemId", "CurrencyName", recipe.ResultItemId);
            return View(recipe);
        }

        // POST: Recipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, [Bind("RecipeId,ResultItemId,CraftingStationName,ResultItemQuantity")] Recipe recipe)
        {
            if (id != recipe.RecipeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recipe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeExists(recipe.RecipeId))
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
            ViewData["CraftingStationName"] = new SelectList(_context.CraftingStation, "CraftingStationName", "CraftingStationName", recipe.CraftingStationName);
            ViewData["ResultItemId"] = new SelectList(_context.Item, "ItemId", "CurrencyName", recipe.ResultItemId);
            return View(recipe);
        }

        // GET: Recipes/Delete/5
        public async Task<IActionResult> Delete(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipe
                .Include(r => r.CraftingStation)
                .Include(r => r.ResultItem)
                .FirstOrDefaultAsync(m => m.RecipeId == id);
            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var recipe = await _context.Recipe.FindAsync(id);
            if (recipe != null)
            {
                _context.Recipe.Remove(recipe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecipeExists(short id)
        {
            return _context.Recipe.Any(e => e.RecipeId == id);
        }
    }
}
