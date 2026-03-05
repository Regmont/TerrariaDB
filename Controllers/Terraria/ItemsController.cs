using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;
using TerrariaDB.ViewModels.Terraria.Item;

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
            var items = await _context.Item
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new ItemItemViewModel
                {
                    Id = i.ItemId.ToString(),
                    Name = i.GameObject.GameObjectName,
                    Sprite = i.GameObject.Sprite
                })
                .ToListAsync();

            var viewModel = new ItemIndexViewModel
            {
                Items = items
            };

            return View(viewModel);
        }

        // GET: Items/Details/5
        public async Task<IActionResult> Details(short id)
        {
            var item = await _context.Item
                .Include(i => i.GameObject)
                .Include(i => i.CurrencyType)
                .Include(i => i.CraftingStation)
                .Include(i => i.SummonedBoss)
                    .ThenInclude(b => b.BossParts)
                        .ThenInclude(bp => bp.HostileEntity)
                            .ThenInclude(he => he.Entity)
                                .ThenInclude(e => e.GameObject)
                .Include(i => i.BossDrops)
                    .ThenInclude(bd => bd.Boss)
                        .ThenInclude(b => b.BossParts)
                            .ThenInclude(bp => bp.HostileEntity)
                                .ThenInclude(he => he.Entity)
                                    .ThenInclude(e => e.GameObject)
                .Include(i => i.EntityDrops)
                    .ThenInclude(ed => ed.Entity)
                        .ThenInclude(e => e.GameObject)
                .Include(i => i.TradeOffers)
                    .ThenInclude(to => to.TownNpc)
                        .ThenInclude(t => t.Entity)
                            .ThenInclude(e => e.GameObject)
                .Include(i => i.TradeOffers)
                    .ThenInclude(to => to.TradeType)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null)
            {
                return NotFound();
            }

            var viewModel = new ItemDetailsViewModel
            {
                ItemId = item.ItemId.ToString(),
                Name = item.GameObject.GameObjectName,
                Description = item.GameObject.Description ?? string.Empty,
                Sprite = item.GameObject.Sprite,
                BasePrice = item.BasePrice,
                CurrencyType = item.CurrencyType.CurrencyName,
                CraftingStationName = item.CraftingStation?.CraftingStationName,
                Transformations = GetTransformations(item.GameObject)
            };

            if (item.SummonedBoss != null)
            {
                viewModel.SummonedBossName = item.SummonedBoss.BossName;
                viewModel.SummonedBossSprite = item.SummonedBoss.BossParts
                    .Select(bp => bp.HostileEntity.Entity.GameObject)
                    .FirstOrDefault()?.Sprite;
            }

            viewModel.DroppedFromBosses = item.BossDrops.Select(bd => new ItemBossDropViewModel
            {
                Name = bd.Boss.BossName,
                Sprite = bd.Boss.BossParts
                    .Select(bp => bp.HostileEntity.Entity.GameObject)
                    .FirstOrDefault()?.Sprite ?? string.Empty
            }).ToList();

            viewModel.DroppedFromEntities = item.EntityDrops.Select(ed => new ItemEntityDropViewModel
            {
                Name = ed.Entity.GameObject.GameObjectName,
                Sprite = ed.Entity.GameObject.Sprite
            }).ToList();

            viewModel.TradedByNpcs = item.TradeOffers.Select(to => new ItemTradeViewModel
            {
                Name = to.TownNpc.Entity.GameObject.GameObjectName,
                Sprite = to.TownNpc.Entity.GameObject.Sprite,
                Quantity = to.Quantity,
                TotalPrice = item.BasePrice * to.Quantity,
                TradeType = to.TradeType.TradeTypeName
            }).ToList();

            return View(viewModel);
        }

        private List<ItemTransformationViewModel> GetTransformations(GameObject gameObject)
        {
            var transformations = new List<ItemTransformationViewModel>();

            var current = gameObject.Transform;
            while (current != null)
            {
                transformations.Add(new ItemTransformationViewModel
                {
                    Name = current.GameObjectName,
                    Sprite = current.Sprite
                });

                current = current.Transform;
            }

            return transformations;
        }

        // GET: Items/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var viewModel = new ItemCreateViewModel();

            viewModel.AvailableCurrencies = _context.CurrencyType
                .Select(ct => new SelectListItem
                {
                    Value = ct.CurrencyName,
                    Text = ct.CurrencyName
                })
                .ToList();

            viewModel.AvailableCraftingStations = _context.CraftingStation
                .Select(cs => new SelectListItem
                {
                    Value = cs.CraftingStationName,
                    Text = cs.CraftingStationName
                })
                .ToList();

            for (int i = 0; i < 4; i++)
            {
                viewModel.Stages.Add(new StageSpriteViewModel());
            }

            return View(viewModel);
        }

        // POST: Items/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ItemCreateViewModel viewModel)
        {
            viewModel.AvailableCurrencies = _context.CurrencyType
                .Select(ct => new SelectListItem
                {
                    Value = ct.CurrencyName,
                    Text = ct.CurrencyName
                })
                .ToList();

            viewModel.AvailableCraftingStations = _context.CraftingStation
                .Select(cs => new SelectListItem
                {
                    Value = cs.CraftingStationName,
                    Text = cs.CraftingStationName
                })
                .ToList();

            var itemIds = new short?[]
            {
        viewModel.FirstItemId,
        viewModel.SecondItemId,
        viewModel.ThirdItemId,
        viewModel.FourthItemId
            };

            var validStages = new List<(short ItemId, StageSpriteViewModel Stage)>();

            for (int i = 0; i < 4; i++)
            {
                if (!string.IsNullOrEmpty(viewModel.Stages[i].Sprite))
                {
                    if (itemIds[i] == null)
                    {
                        ModelState.AddModelError("", $"Item ID for stage {i + 1} is required");
                    }
                    else
                    {
                        if (await _context.Item.AnyAsync(it => it.ItemId == itemIds[i]!.Value))
                        {
                            ModelState.AddModelError("", $"Item with ID {itemIds[i]!.Value} already exists");
                        }
                        validStages.Add((itemIds[i]!.Value, viewModel.Stages[i]));
                    }
                }
            }

            if (!validStages.Any())
            {
                ModelState.AddModelError("Stages", "At least first stage is required");
            }

            if (await _context.GameObject.AnyAsync(go => go.GameObjectName == viewModel.Name))
            {
                ModelState.AddModelError("Name", "An item with this name already exists");
            }

            if (string.IsNullOrEmpty(viewModel.Description))
            {
                ModelState.Remove("Description");
            }

            for (int i = 1; i < viewModel.Stages.Count; i++)
            {
                if (string.IsNullOrEmpty(viewModel.Stages[i].Sprite))
                {
                    ModelState.Remove($"Stages[{i}].Sprite");
                }
            }

            if (viewModel.SecondItemId == null)
                ModelState.Remove("SecondItemId");
            if (viewModel.ThirdItemId == null)
                ModelState.Remove("ThirdItemId");
            if (viewModel.FourthItemId == null)
                ModelState.Remove("FourthItemId");

            if (ModelState.IsValid)
            {
                for (int i = 0; i < validStages.Count; i++)
                {
                    var (itemId, stage) = validStages[i];
                    var gameObjectName = i == 0 ? viewModel.Name : $"{viewModel.Name}_{i + 1}";

                    var gameObject = new GameObject
                    {
                        GameObjectName = gameObjectName,
                        Description = viewModel.Description,
                        Sprite = stage.Sprite,
                        TransformName = i < validStages.Count - 1 ? $"{viewModel.Name}_{i + 2}" : null
                    };

                    _context.GameObject.Add(gameObject);

                    var item = new Item
                    {
                        ItemId = itemId,
                        GameObjectName = gameObject.GameObjectName,
                        BasePrice = i == 0 ? viewModel.BasePrice : 0,
                        CurrencyName = viewModel.CurrencyName,
                        CraftingStationName = i == 0 ? viewModel.CraftingStationName : null
                    };

                    _context.Item.Add(item);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        //// GET: Items/Edit/5
        //[Authorize(Roles = "Admin")]
        //public IActionResult Edit(int id)
        //{
        //    var item = _context.Item
        //        .Include(i => i.GameObject)
        //            .ThenInclude(go => go.Transform)
        //        .Include(i => i.CurrencyType)
        //        .Include(i => i.CraftingStation)
        //        .FirstOrDefault(i => i.ItemId == id);

        //    if (item == null)
        //    {
        //        return NotFound();
        //    }

        //    var viewModel = new ItemEditViewModel
        //    {
        //        ItemId = item.ItemId.ToString(),
        //        Name = item.GameObject.GameObjectName,
        //        Description = item.GameObject.Description ?? string.Empty,
        //        BasePrice = item.BasePrice,
        //        CurrencyName = item.CurrencyName,
        //        CraftingStationName = item.CraftingStationName
        //    };

        //    viewModel.AvailableCurrencies = _context.CurrencyType
        //        .Select(ct => new SelectListItem
        //        {
        //            Value = ct.CurrencyName,
        //            Text = ct.CurrencyName
        //        })
        //        .ToList();

        //    viewModel.AvailableCraftingStations = _context.CraftingStation
        //        .Select(cs => new SelectListItem
        //        {
        //            Value = cs.CraftingStationName,
        //            Text = cs.CraftingStationName
        //        })
        //        .ToList();

        //    viewModel.AvailableItems = _context.Item
        //        .Include(i => i.GameObject)
        //        .Where(i => i.GameObject.TransformedFrom == null)
        //        .Select(i => new SelectListItem
        //        {
        //            Value = i.ItemId.ToString(),
        //            Text = i.GameObject.GameObjectName
        //        })
        //        .ToList();

        //    var currentItem = item;
        //    for (int i = 0; i < 4; i++)
        //    {
        //        if (currentItem != null)
        //        {
        //            viewModel.StageItemIds.Add(currentItem.ItemId.ToString());
        //            currentItem = currentItem.GameObject.Transform != null
        //                ? _context.Item.FirstOrDefault(i => i.GameObjectName == currentItem.GameObject.Transform.GameObjectName)
        //                : null;
        //        }
        //        else
        //        {
        //            viewModel.StageItemIds.Add(string.Empty);
        //        }
        //    }

        //    return View(viewModel);
        //}

        //// POST: Items/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Edit(ItemEditViewModel viewModel)
        //{
        //    viewModel.AvailableCurrencies = _context.CurrencyType
        //        .Select(ct => new SelectListItem
        //        {
        //            Value = ct.CurrencyName,
        //            Text = ct.CurrencyName
        //        })
        //        .ToList();

        //    viewModel.AvailableCraftingStations = _context.CraftingStation
        //        .Select(cs => new SelectListItem
        //        {
        //            Value = cs.CraftingStationName,
        //            Text = cs.CraftingStationName
        //        })
        //        .ToList();

        //    viewModel.AvailableItems = _context.Item
        //        .Include(i => i.GameObject)
        //        .Where(i => i.GameObject.TransformedFrom == null)
        //        .Select(i => new SelectListItem
        //        {
        //            Value = i.ItemId.ToString(),
        //            Text = i.GameObject.GameObjectName
        //        })
        //        .ToList();

        //    if (ModelState.IsValid)
        //    {
        //        var originalItem = await _context.Item
        //            .Include(i => i.GameObject)
        //            .FirstOrDefaultAsync(i => i.ItemId == short.Parse(viewModel.ItemId));

        //        if (originalItem == null)
        //        {
        //            return NotFound();
        //        }

        //        if (originalItem.GameObject.GameObjectName != viewModel.Name &&
        //            await _context.GameObject.AnyAsync(go => go.GameObjectName == viewModel.Name))
        //        {
        //            ModelState.AddModelError("Name", "An item with this name already exists");
        //            return View(viewModel);
        //        }

        //        var existingGameObjects = new List<GameObject>();
        //        var current = originalItem.GameObject;
        //        while (current != null)
        //        {
        //            existingGameObjects.Add(current);
        //            current = await _context.GameObject
        //                .FirstOrDefaultAsync(go => go.GameObjectName == current.TransformName);
        //        }

        //        var validStages = viewModel.StageItemIds
        //            .Where(id => !string.IsNullOrEmpty(id))
        //            .Select(id => short.Parse(id))
        //            .ToList();

        //        var stageItems = new List<Item>();
        //        foreach (var stageItemId in validStages)
        //        {
        //            var stageItem = await _context.Item
        //                .Include(i => i.GameObject)
        //                .FirstOrDefaultAsync(i => i.ItemId == stageItemId);

        //            if (stageItem == null)
        //            {
        //                ModelState.AddModelError("", $"Item with ID {stageItemId} not found");
        //                return View(viewModel);
        //            }
        //            stageItems.Add(stageItem);
        //        }

        //        GameObject? previousGameObject = null;

        //        for (int i = 0; i < validStages.Count; i++)
        //        {
        //            var stageItem = stageItems[i];
        //            var gameObjectName = i == 0 ? viewModel.Name : $"{viewModel.Name}_{i + 1}";

        //            GameObject gameObject;

        //            if (i < existingGameObjects.Count)
        //            {
        //                gameObject = existingGameObjects[i];
        //                gameObject.GameObjectName = gameObjectName;
        //                gameObject.Description = i == 0 ? viewModel.Description : null;
        //                gameObject.Sprite = stageItem.GameObject.Sprite;
        //                gameObject.TransformName = previousGameObject?.GameObjectName;

        //                _context.GameObject.Update(gameObject);
        //            }
        //            else
        //            {
        //                gameObject = new GameObject
        //                {
        //                    GameObjectName = gameObjectName,
        //                    Description = i == 0 ? viewModel.Description : null,
        //                    Sprite = stageItem.GameObject.Sprite,
        //                    TransformName = previousGameObject?.GameObjectName
        //                };

        //                _context.GameObject.Add(gameObject);
        //            }

        //            stageItem.GameObjectName = gameObject.GameObjectName;
        //            if (i == 0)
        //            {
        //                stageItem.BasePrice = viewModel.BasePrice;
        //                stageItem.CurrencyName = viewModel.CurrencyName;
        //                stageItem.CraftingStationName = viewModel.CraftingStationName;
        //            }

        //            _context.Item.Update(stageItem);

        //            previousGameObject = gameObject;
        //        }

        //        for (int i = validStages.Count; i < existingGameObjects.Count; i++)
        //        {
        //            var extraGo = existingGameObjects[i];

        //            var extraItem = await _context.Item
        //                .FirstOrDefaultAsync(item => item.GameObjectName == extraGo.GameObjectName);

        //            if (extraItem != null)
        //            {
        //                _context.Item.Remove(extraItem);
        //            }

        //            _context.GameObject.Remove(extraGo);
        //        }

        //        try
        //        {
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ItemExists(short.Parse(viewModel.ItemId)))
        //            {
        //                return NotFound();
        //            }
        //            throw;
        //        }

        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(viewModel);
        //}

        // GET: Items/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(short id)
        {
            var item = await _context.Item
                .Include(i => i.GameObject)
                .Include(i => i.ResultRecipes)
                .Include(i => i.CraftingStation)
                    .ThenInclude(cs => cs.Items)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null)
            {
                return NotFound();
            }

            var viewModel = new ItemDeleteViewModel
            {
                ItemId = item.ItemId.ToString(),
                Name = item.GameObject.GameObjectName,
                Sprite = item.GameObject.Sprite,
                HasRelatedRecipes = item.ResultRecipes.Any(),
                IsLastCraftingStationItem = item.CraftingStation != null &&
                                             item.CraftingStation.Items.Count == 1
            };

            return View(viewModel);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(ItemDeleteViewModel viewModel)
        {
            var item = await _context.Item
                .Include(i => i.GameObject)
                .Include(i => i.CraftingStation)
                    .ThenInclude(cs => cs!.Items)
                .Include(i => i.BossDrops)
                .Include(i => i.EntityDrops)
                .Include(i => i.TradeOffers)
                .Include(i => i.ResultRecipes)
                    .ThenInclude(r => r.RecipeItems)
                .FirstOrDefaultAsync(i => i.ItemId == short.Parse(viewModel.ItemId));

            if (item == null)
            {
                return NotFound();
            }

            if (item.CraftingStation != null && item.CraftingStation.Items.Count == 1)
            {
                var errorViewModel = new ItemDeleteViewModel
                {
                    ItemId = item.ItemId.ToString(),
                    Name = item.GameObject.GameObjectName,
                    Sprite = item.GameObject.Sprite,
                    HasRelatedRecipes = item.ResultRecipes.Any(),
                    IsLastCraftingStationItem = true
                };
                return View(errorViewModel);
            }

            var allGameObjects = new List<GameObject>();
            var allBossDrops = new List<BossDrop>();
            var allEntityDrops = new List<EntityDrop>();
            var allTradeOffers = new List<TradeOffer>();
            var allRecipeItems = new List<RecipeItems>();
            var allRecipes = new List<Recipe>();

            await CollectItemData(item.GameObject, allGameObjects, allBossDrops, allEntityDrops,
                allTradeOffers, allRecipeItems, allRecipes);

            _context.BossDrop.RemoveRange(allBossDrops);
            _context.EntityDrop.RemoveRange(allEntityDrops);
            _context.TradeOffer.RemoveRange(allTradeOffers);
            _context.RecipeItems.RemoveRange(allRecipeItems);
            _context.Recipe.RemoveRange(allRecipes);

            foreach (var go in allGameObjects)
            {
                _context.GameObject.Remove(go);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task CollectItemData(GameObject startGameObject,
            List<GameObject> gameObjects,
            List<BossDrop> bossDrops,
            List<EntityDrop> entityDrops,
            List<TradeOffer> tradeOffers,
            List<RecipeItems> recipeItems,
            List<Recipe> recipes)
        {
            var current = startGameObject;

            while (current != null && !gameObjects.Contains(current))
            {
                gameObjects.Add(current);

                var itemAtStage = await _context.Item
                    .Include(i => i.BossDrops)
                    .Include(i => i.EntityDrops)
                    .Include(i => i.TradeOffers)
                    .Include(i => i.ResultRecipes)
                        .ThenInclude(r => r.RecipeItems)
                    .FirstOrDefaultAsync(i => i.GameObjectName == current.GameObjectName);

                if (itemAtStage != null)
                {
                    bossDrops.AddRange(itemAtStage.BossDrops);
                    entityDrops.AddRange(itemAtStage.EntityDrops);
                    tradeOffers.AddRange(itemAtStage.TradeOffers);

                    foreach (var recipe in itemAtStage.ResultRecipes)
                    {
                        recipeItems.AddRange(recipe.RecipeItems);
                        recipes.Add(recipe);
                    }
                }

                current = await _context.GameObject
                    .FirstOrDefaultAsync(go => go.GameObjectName == current.TransformName);
            }
        }

        private bool ItemExists(short id)
        {
            return _context.Item.Any(e => e.ItemId == id);
        }
    }
}
