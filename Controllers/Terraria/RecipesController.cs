using Microsoft.AspNetCore.Authorization;
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
                    .ThenInclude(cs => cs!.Items)
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
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var viewModel = new RecipeCreateViewModel();

            viewModel.AvailableItems = _context.Item
                .Include(i => i.GameObject)
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.ItemId.ToString(),
                    Text = i.GameObject.GameObjectName
                })
                .ToList();

            viewModel.AvailableCraftingStations = _context.CraftingStation
                .Select(cs => new SelectListItem
                {
                    Value = cs.CraftingStationName,
                    Text = cs.CraftingStationName
                })
                .ToList();

            for (int i = 0; i < 8; i++)
            {
                viewModel.Ingredients.Add(new RecipeCreateIngredientViewModel());
            }

            return View(viewModel);
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(RecipeCreateViewModel viewModel)
        {
            viewModel.AvailableItems = _context.Item
                .Include(i => i.GameObject)
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.ItemId.ToString(),
                    Text = i.GameObject.GameObjectName
                })
                .ToList();

            viewModel.AvailableCraftingStations = _context.CraftingStation
                .Select(cs => new SelectListItem
                {
                    Value = cs.CraftingStationName,
                    Text = cs.CraftingStationName
                })
                .ToList();

            if (ModelState.IsValid)
            {
                var ingredientIds = viewModel.Ingredients
                    .Where(i => !string.IsNullOrEmpty(i.ItemId) && i.Quantity > 0)
                    .Select(i => short.Parse(i.ItemId))
                    .OrderBy(id => id)
                    .ToList();

                var existingRecipe = await _context.Recipe
                    .Include(r => r.RecipeItems)
                    .Where(r => r.ResultItemId == short.Parse(viewModel.ResultItemId))
                    .Where(r => r.CraftingStationName == viewModel.CraftingStationName)
                    .Where(r => r.RecipeItems.All(ri => ingredientIds.Contains(ri.ItemId)))
                    .Where(r => ingredientIds.All(id => r.RecipeItems.Any(ri => ri.ItemId == id)))
                    .FirstOrDefaultAsync();

                if (existingRecipe != null)
                {
                    ModelState.AddModelError("", "A recipe with these items already exists");
                    return View(viewModel);
                }

                var recipe = new Recipe
                {
                    ResultItemId = short.Parse(viewModel.ResultItemId),
                    ResultItemQuantity = viewModel.ResultItemQuantity,
                    CraftingStationName = viewModel.CraftingStationName
                };

                _context.Add(recipe);
                await _context.SaveChangesAsync();

                foreach (var ingredient in viewModel.Ingredients.Where(i => !string.IsNullOrEmpty(i.ItemId) && i.Quantity > 0))
                {
                    var recipeItem = new RecipeItems
                    {
                        RecipeId = recipe.RecipeId,
                        ItemId = short.Parse(ingredient.ItemId),
                        Quantity = (short)ingredient.Quantity
                    };
                    _context.RecipeItems.Add(recipeItem);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Recipes/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var recipe = _context.Recipe
                .Include(r => r.ResultItem)
                    .ThenInclude(i => i.GameObject)
                .Include(r => r.CraftingStation)
                .Include(r => r.RecipeItems)
                    .ThenInclude(ri => ri.Item)
                    .ThenInclude(i => i.GameObject)
                .FirstOrDefault(r => r.RecipeId == id);

            if (recipe == null)
            {
                return NotFound();
            }

            var viewModel = new RecipeEditViewModel
            {
                RecipeId = recipe.RecipeId.ToString(),
                ResultItemId = recipe.ResultItemId.ToString(),
                ResultItemQuantity = recipe.ResultItemQuantity,
                CraftingStationName = recipe.CraftingStationName
            };

            viewModel.AvailableItems = _context.Item
                .Include(i => i.GameObject)
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.ItemId.ToString(),
                    Text = i.GameObject.GameObjectName
                })
                .ToList();

            viewModel.AvailableCraftingStations = _context.CraftingStation
                .Select(cs => new SelectListItem
                {
                    Value = cs.CraftingStationName,
                    Text = cs.CraftingStationName
                })
                .ToList();

            for (int i = 0; i < 8; i++)
            {
                viewModel.Ingredients.Add(new RecipeEditIngredientViewModel());
            }

            var recipeItems = recipe.RecipeItems.ToList();
            for (int i = 0; i < recipeItems.Count && i < 8; i++)
            {
                viewModel.Ingredients[i].ItemId = recipeItems[i].ItemId.ToString();
                viewModel.Ingredients[i].Quantity = recipeItems[i].Quantity;
            }

            return View(viewModel);
        }

        // POST: Recipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(RecipeEditViewModel viewModel)
        {
            viewModel.AvailableItems = _context.Item
                .Include(i => i.GameObject)
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.ItemId.ToString(),
                    Text = i.GameObject.GameObjectName
                })
                .ToList();

            viewModel.AvailableCraftingStations = _context.CraftingStation
                .Select(cs => new SelectListItem
                {
                    Value = cs.CraftingStationName,
                    Text = cs.CraftingStationName
                })
                .ToList();

            if (ModelState.IsValid)
            {
                var recipe = await _context.Recipe
                    .Include(r => r.RecipeItems)
                    .FirstOrDefaultAsync(r => r.RecipeId == short.Parse(viewModel.RecipeId));

                if (recipe == null)
                {
                    return NotFound();
                }

                var ingredientIds = viewModel.Ingredients
                    .Where(i => !string.IsNullOrEmpty(i.ItemId) && i.Quantity > 0)
                    .Select(i => short.Parse(i.ItemId))
                    .OrderBy(id => id)
                    .ToList();

                var duplicateExists = await _context.Recipe
                    .Include(r => r.RecipeItems)
                    .Where(r => r.RecipeId != recipe.RecipeId)
                    .Where(r => r.ResultItemId == short.Parse(viewModel.ResultItemId))
                    .Where(r => r.CraftingStationName == viewModel.CraftingStationName)
                    .Where(r => r.RecipeItems.All(ri => ingredientIds.Contains(ri.ItemId)))
                    .Where(r => ingredientIds.All(id => r.RecipeItems.Any(ri => ri.ItemId == id)))
                    .AnyAsync();

                if (duplicateExists)
                {
                    ModelState.AddModelError("", "A recipe with these items already exists");
                    return View(viewModel);
                }

                recipe.ResultItemId = short.Parse(viewModel.ResultItemId);
                recipe.ResultItemQuantity = viewModel.ResultItemQuantity;
                recipe.CraftingStationName = viewModel.CraftingStationName;

                _context.RecipeItems.RemoveRange(recipe.RecipeItems);

                foreach (var ingredient in viewModel.Ingredients.Where(i => !string.IsNullOrEmpty(i.ItemId) && i.Quantity > 0))
                {
                    var recipeItem = new RecipeItems
                    {
                        RecipeId = recipe.RecipeId,
                        ItemId = short.Parse(ingredient.ItemId),
                        Quantity = (short)ingredient.Quantity
                    };
                    _context.RecipeItems.Add(recipeItem);
                }

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
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Recipes/Delete/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
