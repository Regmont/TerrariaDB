using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;
using TerrariaDB.ViewModels.Terraria.Recipe;

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
        public async Task<IActionResult> Index(string resultItem, string ingredient1, string ingredient2, string craftingStation)
        {
            var query = _context.Recipe
                .Include(r => r.ResultItem)
                    .ThenInclude(i => i.GameObject)
                .Include(r => r.RecipeItems)
                    .ThenInclude(ri => ri.Item)
                        .ThenInclude(i => i.GameObject)
                .Include(r => r.CraftingStation)
                    .ThenInclude(cs => cs.Items)
                        .ThenInclude(i => i.GameObject)
                .AsQueryable();

            if (!string.IsNullOrEmpty(resultItem))
            {
                query = query.Where(r => r.ResultItem.GameObject.GameObjectName == resultItem);
            }

            if (!string.IsNullOrEmpty(ingredient1) || !string.IsNullOrEmpty(ingredient2))
            {
                var ingredients = new[] { ingredient1, ingredient2 }.Where(i => !string.IsNullOrEmpty(i)).ToList();
                foreach (var ingredient in ingredients)
                {
                    query = query.Where(r => r.RecipeItems.Any(ri => ri.Item.GameObject.GameObjectName == ingredient));
                }
            }

            if (!string.IsNullOrEmpty(craftingStation))
            {
                query = query.Where(r => r.CraftingStation!.CraftingStationName == craftingStation);
            }

            var recipes = await query
                .Select(r => new RecipeItemViewModel
                {
                    Id = r.RecipeId.ToString(),
                    ResultItem = new RecipeItemInfoViewModel
                    {
                        Name = r.ResultItem.GameObject.GameObjectName,
                        Sprite = r.ResultItem.GameObject.Sprite
                    },
                    ResultItemQuantity = r.ResultItemQuantity,
                    Ingredients = r.RecipeItems.Select(ri => new RecipeIngredientViewModel
                    {
                        Name = ri.Item.GameObject.GameObjectName,
                        Sprite = ri.Item.GameObject.Sprite,
                        Quantity = ri.Quantity
                    }).ToList(),
                    CraftingStation = r.CraftingStation != null ? new RecipeStationViewModel
                    {
                        Name = r.CraftingStation.CraftingStationName,
                        Sprite = r.CraftingStation.Items.FirstOrDefault()!.GameObject.Sprite
                    } : null
                })
                .ToListAsync();

            var resultItemOptions = await _context.Item
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.GameObject.GameObjectName,
                    Text = i.GameObject.GameObjectName
                })
                .Distinct()
                .ToListAsync();

            var ingredientOptions = await _context.Item
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.GameObject.GameObjectName,
                    Text = i.GameObject.GameObjectName
                })
                .Distinct()
                .ToListAsync();

            var stationOptions = await _context.CraftingStation
                .Select(cs => new SelectListItem
                {
                    Value = cs.CraftingStationName,
                    Text = cs.CraftingStationName
                })
                .ToListAsync();

            var viewModel = new RecipeIndexViewModel
            {
                Recipes = recipes,
                ResultItemFilterOptions = resultItemOptions,
                IngredientFilterOptions = ingredientOptions,
                CraftingStationFilterOptions = stationOptions
            };

            return View(viewModel);
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
        public async Task<IActionResult> Delete(short id)
        {
            var recipe = await _context.Recipe
                .FirstOrDefaultAsync(r => r.RecipeId == id);

            if (recipe == null)
            {
                return NotFound();
            }

            var viewModel = new RecipeDeleteViewModel
            {
                RecipeId = recipe.RecipeId.ToString()
            };

            return View(viewModel);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var recipe = await _context.Recipe
                .Include(r => r.RecipeItems)
                .FirstOrDefaultAsync(r => r.RecipeId == id);

            if (recipe == null)
            {
                return NotFound();
            }

            _context.RecipeItems.RemoveRange(recipe.RecipeItems);
            _context.Recipe.Remove(recipe);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool RecipeExists(short id)
        {
            return _context.Recipe.Any(e => e.RecipeId == id);
        }
    }
}
