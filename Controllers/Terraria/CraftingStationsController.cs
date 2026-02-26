using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;
using TerrariaDB.ViewModels.Terraria.CraftingStation;

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
            var stations = await _context.CraftingStation
                .Select(cs => new CraftingStationItemViewModel
                {
                    Name = cs.CraftingStationName,
                    Sprite = cs.Items
                        .Select(i => i.GameObject)
                        .FirstOrDefault(go => go.TransformedFrom == null)!.Sprite
                })
                .ToListAsync();

            var viewModel = new CraftingStationIndexViewModel
            {
                CraftingStations = stations
            };

            return View(viewModel);
        }

        // GET: CraftingStations/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var craftingStation = await _context.CraftingStation
                .Include(cs => cs.Items)
                    .ThenInclude(i => i.GameObject)
                .FirstOrDefaultAsync(cs => cs.CraftingStationName == id);

            if (craftingStation == null)
            {
                return NotFound();
            }

            var viewModel = new CraftingStationDetailsViewModel
            {
                CraftingStationName = craftingStation.CraftingStationName,
                Items = craftingStation.Items.Select(i => new CraftingStationItemDetailsViewModel
                {
                    Name = i.GameObject.GameObjectName,
                    Sprite = i.GameObject.Sprite
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: CraftingStations/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var viewModel = new CraftingStationCreateViewModel();

            viewModel.AvailableItems = _context.Item
                .Include(i => i.GameObject)
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.ItemId.ToString(),
                    Text = i.GameObject.GameObjectName
                })
                .ToList();

            return View(viewModel);
        }

        // POST: CraftingStations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CraftingStationCreateViewModel viewModel)
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

            if (ModelState.IsValid)
            {
                if (await _context.CraftingStation.AnyAsync(cs => cs.CraftingStationName == viewModel.CraftingStationName))
                {
                    ModelState.AddModelError("CraftingStationName", "A crafting station with this name already exists");
                    return View(viewModel);
                }

                var craftingStation = new CraftingStation
                {
                    CraftingStationName = viewModel.CraftingStationName
                };

                _context.Add(craftingStation);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(viewModel.SelectedItemId))
                {
                    var item = await _context.Item.FindAsync(short.Parse(viewModel.SelectedItemId));
                    if (item != null)
                    {
                        item.CraftingStationName = craftingStation.CraftingStationName;
                        _context.Update(item);
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: CraftingStations/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(string name)
        {
            var craftingStation = _context.CraftingStation
                .Include(cs => cs.Items)
                    .ThenInclude(i => i.GameObject)
                .FirstOrDefault(cs => cs.CraftingStationName == name);

            if (craftingStation == null)
            {
                return NotFound();
            }

            var viewModel = new CraftingStationEditViewModel
            {
                OriginalCraftingStationName = craftingStation.CraftingStationName,
                CraftingStationName = craftingStation.CraftingStationName
            };

            viewModel.AvailableItems = _context.Item
                .Include(i => i.GameObject)
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.ItemId.ToString(),
                    Text = i.GameObject.GameObjectName,
                    Selected = craftingStation.Items.Any(ci => ci.ItemId == i.ItemId)
                })
                .ToList();

            viewModel.SelectedItemId = craftingStation.Items
                .FirstOrDefault()?.ItemId.ToString() ?? string.Empty;

            return View(viewModel);
        }

        // POST: CraftingStations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(CraftingStationEditViewModel viewModel)
        {
            viewModel.AvailableItems = _context.Item
                .Include(i => i.GameObject)
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.ItemId.ToString(),
                    Text = i.GameObject.GameObjectName,
                    Selected = i.ItemId.ToString() == viewModel.SelectedItemId
                })
                .ToList();

            if (ModelState.IsValid)
            {
                var craftingStation = await _context.CraftingStation
                    .Include(cs => cs.Items)
                    .FirstOrDefaultAsync(cs => cs.CraftingStationName == viewModel.OriginalCraftingStationName);

                if (craftingStation == null)
                {
                    return NotFound();
                }

                if (viewModel.OriginalCraftingStationName != viewModel.CraftingStationName &&
                    await _context.CraftingStation.AnyAsync(cs => cs.CraftingStationName == viewModel.CraftingStationName))
                {
                    ModelState.AddModelError("CraftingStationName", "A crafting station with this name already exists");
                    return View(viewModel);
                }

                var oldItems = craftingStation.Items.ToList();
                foreach (var item in oldItems)
                {
                    item.CraftingStationName = null;
                    _context.Update(item);
                }

                craftingStation.CraftingStationName = viewModel.CraftingStationName;

                if (!string.IsNullOrEmpty(viewModel.SelectedItemId))
                {
                    var newItem = await _context.Item.FindAsync(short.Parse(viewModel.SelectedItemId));
                    if (newItem != null)
                    {
                        newItem.CraftingStationName = craftingStation.CraftingStationName;
                        _context.Update(newItem);
                    }
                }

                try
                {
                    _context.Update(craftingStation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CraftingStationExists(viewModel.OriginalCraftingStationName))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: CraftingStations/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var craftingStation = await _context.CraftingStation
                .Include(cs => cs.Items)
                    .ThenInclude(i => i.GameObject)
                .Include(cs => cs.Recipes)
                .FirstOrDefaultAsync(cs => cs.CraftingStationName == id);

            if (craftingStation == null)
            {
                return NotFound();
            }

            var viewModel = new CraftingStationDeleteViewModel
            {
                CraftingStationName = craftingStation.CraftingStationName,
                Sprite = craftingStation.Items.FirstOrDefault()?.GameObject.Sprite ?? string.Empty,
                HasRelatedRecipes = craftingStation.Recipes.Any()
            };

            return View(viewModel);
        }

        // POST: CraftingStations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var craftingStation = await _context.CraftingStation
                .Include(cs => cs.Items)
                .Include(cs => cs.Recipes)
                    .ThenInclude(r => r.RecipeItems)
                .FirstOrDefaultAsync(cs => cs.CraftingStationName == id);

            if (craftingStation == null)
            {
                return NotFound();
            }

            var allRecipeItems = new List<RecipeItems>();
            foreach (var recipe in craftingStation.Recipes)
            {
                allRecipeItems.AddRange(recipe.RecipeItems);
            }

            _context.RecipeItems.RemoveRange(allRecipeItems);
            _context.Recipe.RemoveRange(craftingStation.Recipes);
            _context.CraftingStation.Remove(craftingStation);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool CraftingStationExists(string id)
        {
            return _context.CraftingStation.Any(e => e.CraftingStationName == id);
        }
    }
}
