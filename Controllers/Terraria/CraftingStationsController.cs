using Microsoft.AspNetCore.Mvc;
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
