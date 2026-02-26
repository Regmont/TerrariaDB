using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;
using TerrariaDB.ViewModels.Terraria.TownNpc;

namespace TerrariaDB.Controllers.Terraria
{
    public class TownNpcsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const int DefaultHp = 250;
        private const int DefaultDefense = 15;

        public TownNpcsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TownNpcs
        public async Task<IActionResult> Index()
        {
            var townNpcs = await _context.TownNpc
                .Where(t => t.Entity.GameObject.TransformedFrom == null)
                .Select(t => new TownNpcItemViewModel
                {
                    Id = t.TownNpcId.ToString(),
                    Name = t.Entity.GameObject.GameObjectName,
                    Sprite = t.Entity.GameObject.Sprite
                })
                .ToListAsync();

            var viewModel = new TownNpcIndexViewModel
            {
                TownNpcs = townNpcs
            };

            return View(viewModel);
        }

        // GET: TownNpcs/Details/5
        public async Task<IActionResult> Details(byte id)
        {
            var townNpc = await _context.TownNpc
                .Include(t => t.Entity)
                    .ThenInclude(e => e.GameObject)
                .Include(t => t.Entity)
                    .ThenInclude(e => e.EntityDrops)
                        .ThenInclude(ed => ed.Item)
                            .ThenInclude(i => i.GameObject)
                .Include(t => t.TradeOffers)
                    .ThenInclude(to => to.Item)
                        .ThenInclude(i => i.GameObject)
                .Include(t => t.TradeOffers)
                    .ThenInclude(to => to.TradeType)
                .FirstOrDefaultAsync(t => t.TownNpcId == id);

            if (townNpc == null)
            {
                return NotFound();
            }

            var viewModel = new TownNpcDetailsViewModel
            {
                TownNpcId = townNpc.TownNpcId.ToString(),
                Name = townNpc.Entity.GameObject.GameObjectName,
                Description = townNpc.Entity.GameObject.Description ?? string.Empty,
                Sprite = townNpc.Entity.GameObject.Sprite,
                EntityId = townNpc.EntityId.ToString(),
                Hp = townNpc.Entity.Hp ?? 0,
                Defense = townNpc.Entity.Defense,
                Drops = townNpc.Entity.EntityDrops.Select(ed => new TownNpcDropViewModel
                {
                    Name = ed.Item.GameObject.GameObjectName,
                    Sprite = ed.Item.GameObject.Sprite,
                    Quantity = ed.Quantity
                }).ToList(),
                Trades = townNpc.TradeOffers.Select(to => new TownNpcTradeViewModel
                {
                    Name = to.Item.GameObject.GameObjectName,
                    Sprite = to.Item.GameObject.Sprite,
                    Quantity = to.Quantity,
                    TotalPrice = to.Item.BasePrice * to.Quantity,
                    TradeType = to.TradeType.TradeTypeName
                }).ToList(),
                Transformations = GetTransformations(townNpc.Entity.GameObject, townNpc)
            };

            return View(viewModel);
        }

        private List<TownNpcTransformationViewModel> GetTransformations(GameObject gameObject, TownNpc townNpc)
        {
            var transformations = new List<TownNpcTransformationViewModel>();

            var current = gameObject.Transform;
            while (current != null)
            {
                transformations.Add(new TownNpcTransformationViewModel
                {
                    Name = current.GameObjectName,
                    Sprite = current.Sprite,
                    EntityId = townNpc.EntityId.ToString(),
                    Hp = townNpc.Entity.Hp ?? 0,
                    Defense = townNpc.Entity.Defense
                });

                current = current.Transform;
            }

            return transformations;
        }

        // GET: TownNpcs/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var viewModel = new TownNpcCreateViewModel();

            viewModel.AvailableItems = _context.Item
                .Include(i => i.GameObject)
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.ItemId.ToString(),
                    Text = i.GameObject.GameObjectName
                })
                .ToList();

            viewModel.AvailableTradeTypes = _context.TradeType
                .Select(tt => new SelectListItem
                {
                    Value = tt.TradeTypeName,
                    Text = tt.TradeTypeName
                })
                .ToList();

            for (int i = 0; i < 4; i++)
            {
                viewModel.Stages.Add(new TownNpcCreateStageViewModel
                {
                    Hp = DefaultHp,
                    Defense = DefaultDefense
                });
            }

            for (int i = 0; i < 5; i++)
            {
                viewModel.Drops.Add(new TownNpcDropCreateViewModel());
            }

            for (int i = 0; i < 15; i++)
            {
                viewModel.Trades.Add(new TownNpcTradeCreateViewModel());
            }

            return View(viewModel);
        }

        // POST: TownNpcs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(TownNpcCreateViewModel viewModel)
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

            viewModel.AvailableTradeTypes = _context.TradeType
                .Select(tt => new SelectListItem
                {
                    Value = tt.TradeTypeName,
                    Text = tt.TradeTypeName
                })
                .ToList();

            if (ModelState.IsValid)
            {
                var filledStages = viewModel.Stages
                    .Where(s => !string.IsNullOrEmpty(s.Sprite))
                    .ToList();

                if (!filledStages.Any())
                {
                    ModelState.AddModelError("", "At least one stage must be filled");
                    return View(viewModel);
                }

                var allEntityIds = filledStages.Select(s => s.EntityId).ToList();
                if (allEntityIds.Count != allEntityIds.Distinct().Count())
                {
                    ModelState.AddModelError("", "Entity IDs must be unique across all stages");
                    return View(viewModel);
                }

                var allGameObjectNames = filledStages
                    .Select((s, index) => index == 0 ? viewModel.Name : $"{viewModel.Name}_{index + 1}")
                    .ToList();

                foreach (var name in allGameObjectNames)
                {
                    if (await _context.GameObject.AnyAsync(go => go.GameObjectName == name))
                    {
                        ModelState.AddModelError("", $"Game object with name '{name}' already exists");
                        return View(viewModel);
                    }
                }

                var allSprites = filledStages.Select(s => s.Sprite).ToList();
                foreach (var sprite in allSprites)
                {
                    if (await _context.GameObject.AnyAsync(go => go.Sprite == sprite))
                    {
                        ModelState.AddModelError("", $"Sprite '{sprite}' already exists");
                        return View(viewModel);
                    }
                }

                var validTrades = viewModel.Trades
                    .Where(t => !string.IsNullOrEmpty(t.ItemId) && !string.IsNullOrEmpty(t.TradeType) && t.Quantity > 0)
                    .ToList();

                var tradeKeys = validTrades.Select(t => $"{t.ItemId}_{t.TradeType}").ToList();
                if (tradeKeys.Count != tradeKeys.Distinct().Count())
                {
                    ModelState.AddModelError("", "Duplicate trades (same item and trade type) are not allowed");
                    return View(viewModel);
                }

                GameObject? previousGameObject = null;
                Entity? previousEntity = null;

                for (int i = 0; i < filledStages.Count; i++)
                {
                    var stage = filledStages[i];
                    var gameObjectName = i == 0 ? viewModel.Name : $"{viewModel.Name}_{i + 1}";

                    var gameObject = new GameObject
                    {
                        GameObjectName = gameObjectName,
                        Description = i == 0 ? viewModel.Description : null,
                        Sprite = stage.Sprite,
                        TransformName = previousGameObject?.GameObjectName
                    };

                    _context.GameObject.Add(gameObject);
                    await _context.SaveChangesAsync();

                    var entity = new Entity
                    {
                        EntityId = stage.EntityId,
                        GameObjectName = gameObject.GameObjectName,
                        Hp = stage.Hp,
                        Defense = (short)stage.Defense
                    };

                    _context.Entity.Add(entity);
                    await _context.SaveChangesAsync();

                    var townNpc = new TownNpc
                    {
                        EntityId = entity.EntityId
                    };

                    _context.TownNpc.Add(townNpc);
                    await _context.SaveChangesAsync();

                    if (i == 0)
                    {
                        foreach (var drop in viewModel.Drops.Where(d => !string.IsNullOrEmpty(d.ItemId) && d.Quantity > 0))
                        {
                            var entityDrop = new EntityDrop
                            {
                                EntityId = entity.EntityId,
                                ItemId = short.Parse(drop.ItemId),
                                Quantity = (short)drop.Quantity
                            };
                            _context.EntityDrop.Add(entityDrop);
                        }
                    }

                    if (i == 0)
                    {
                        foreach (var trade in validTrades)
                        {
                            var tradeOffer = new TradeOffer
                            {
                                TownNpcId = townNpc.TownNpcId,
                                ItemId = short.Parse(trade.ItemId),
                                TradeTypeName = trade.TradeType,
                                Quantity = (short)trade.Quantity
                            };
                            _context.TradeOffer.Add(tradeOffer);
                        }
                    }

                    previousGameObject = gameObject;
                    previousEntity = entity;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: TownNpcs/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(byte id)
        {
            var townNpc = _context.TownNpc
                .Include(tn => tn.Entity)
                    .ThenInclude(e => e.GameObject)
                .Include(tn => tn.Entity)
                    .ThenInclude(e => e.EntityDrops)
                    .ThenInclude(ed => ed.Item)
                    .ThenInclude(i => i.GameObject)
                .Include(tn => tn.TradeOffers)
                    .ThenInclude(to => to.Item)
                    .ThenInclude(i => i.GameObject)
                .Include(tn => tn.TradeOffers)
                    .ThenInclude(to => to.TradeType)
                .FirstOrDefault(tn => tn.TownNpcId == id);

            if (townNpc == null)
            {
                return NotFound();
            }

            var viewModel = new TownNpcEditViewModel
            {
                TownNpcId = townNpc.TownNpcId.ToString(),
                Name = townNpc.Entity.GameObject.GameObjectName,
                Description = townNpc.Entity.GameObject.Description ?? string.Empty
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

            viewModel.AvailableTradeTypes = _context.TradeType
                .Select(tt => new SelectListItem
                {
                    Value = tt.TradeTypeName,
                    Text = tt.TradeTypeName
                })
                .ToList();

            for (int i = 0; i < 4; i++)
            {
                var stage = new TownNpcEditStageViewModel();

                if (i == 0)
                {
                    stage.Sprite = townNpc.Entity.GameObject.Sprite;
                    stage.Hp = townNpc.Entity.Hp ?? 250;
                    stage.Defense = townNpc.Entity.Defense;
                    stage.EntityId = townNpc.Entity.EntityId;
                }
                else
                {
                    stage.Hp = 250;
                    stage.Defense = 15;
                }

                viewModel.Stages.Add(stage);
            }

            var drops = townNpc.Entity.EntityDrops.ToList();
            for (int i = 0; i < 5; i++)
            {
                var drop = new TownNpcDropEditViewModel();
                if (i < drops.Count)
                {
                    drop.ItemId = drops[i].ItemId.ToString();
                    drop.Quantity = drops[i].Quantity;
                }
                viewModel.Drops.Add(drop);
            }

            var trades = townNpc.TradeOffers.ToList();
            for (int i = 0; i < 15; i++)
            {
                var trade = new TownNpcTradeEditViewModel();
                if (i < trades.Count)
                {
                    trade.ItemId = trades[i].ItemId.ToString();
                    trade.Quantity = trades[i].Quantity;
                    trade.TradeType = trades[i].TradeTypeName;
                }
                viewModel.Trades.Add(trade);
            }

            return View(viewModel);
        }

        // POST: TownNpcs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(TownNpcEditViewModel viewModel)
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

            viewModel.AvailableTradeTypes = _context.TradeType
                .Select(tt => new SelectListItem
                {
                    Value = tt.TradeTypeName,
                    Text = tt.TradeTypeName
                })
                .ToList();

            if (ModelState.IsValid)
            {
                var originalTownNpc = await _context.TownNpc
                    .Include(t => t.Entity)
                        .ThenInclude(e => e.GameObject)
                    .Include(t => t.Entity)
                        .ThenInclude(e => e.EntityDrops)
                    .Include(t => t.TradeOffers)
                    .FirstOrDefaultAsync(t => t.TownNpcId == byte.Parse(viewModel.TownNpcId));

                if (originalTownNpc == null)
                {
                    return NotFound();
                }

                var existingGameObjects = new List<GameObject>();
                var current = originalTownNpc.Entity.GameObject;
                while (current != null)
                {
                    existingGameObjects.Add(current);
                    current = await _context.GameObject
                        .FirstOrDefaultAsync(go => go.GameObjectName == current.TransformName);
                }

                var filledStages = viewModel.Stages
                    .Where(s => !string.IsNullOrEmpty(s.Sprite))
                    .ToList();

                if (!filledStages.Any())
                {
                    ModelState.AddModelError("", "At least one stage must be filled");
                    return View(viewModel);
                }

                var allEntityIds = filledStages.Select(s => s.EntityId).ToList();
                if (allEntityIds.Count != allEntityIds.Distinct().Count())
                {
                    ModelState.AddModelError("", "Entity IDs must be unique across all stages");
                    return View(viewModel);
                }

                var allGameObjectNames = filledStages
                    .Select((s, index) => index == 0 ? viewModel.Name : $"{viewModel.Name}_{index + 1}")
                    .ToList();

                foreach (var name in allGameObjectNames)
                {
                    if (!existingGameObjects.Any(go => go.GameObjectName == name) &&
                        await _context.GameObject.AnyAsync(go => go.GameObjectName == name))
                    {
                        ModelState.AddModelError("", $"Game object with name '{name}' already exists");
                        return View(viewModel);
                    }
                }

                var allSprites = filledStages.Select(s => s.Sprite).ToList();
                foreach (var sprite in allSprites)
                {
                    if (!existingGameObjects.Any(go => go.Sprite == sprite) &&
                        await _context.GameObject.AnyAsync(go => go.Sprite == sprite))
                    {
                        ModelState.AddModelError("", $"Sprite '{sprite}' already exists");
                        return View(viewModel);
                    }
                }

                var validTrades = viewModel.Trades
                    .Where(t => !string.IsNullOrEmpty(t.ItemId) && !string.IsNullOrEmpty(t.TradeType) && t.Quantity > 0)
                    .ToList();

                var tradeKeys = validTrades.Select(t => $"{t.ItemId}_{t.TradeType}").ToList();
                if (tradeKeys.Count != tradeKeys.Distinct().Count())
                {
                    ModelState.AddModelError("", "Duplicate trades (same item and trade type) are not allowed");
                    return View(viewModel);
                }

                _context.TradeOffer.RemoveRange(originalTownNpc.TradeOffers);

                foreach (var go in existingGameObjects)
                {
                    var entity = await _context.Entity
                        .Include(e => e.EntityDrops)
                        .Include(e => e.TownNpc)
                        .FirstOrDefaultAsync(e => e.GameObjectName == go.GameObjectName);

                    if (entity != null)
                    {
                        _context.EntityDrop.RemoveRange(entity.EntityDrops);
                        if (entity.TownNpc != null)
                        {
                            _context.TownNpc.Remove(entity.TownNpc);
                        }
                        _context.Entity.Remove(entity);
                    }
                    _context.GameObject.Remove(go);
                }
                await _context.SaveChangesAsync();

                GameObject? previousGameObject = null;
                Entity? previousEntity = null;

                for (int i = 0; i < filledStages.Count; i++)
                {
                    var stage = filledStages[i];
                    var gameObjectName = i == 0 ? viewModel.Name : $"{viewModel.Name}_{i + 1}";

                    var gameObject = new GameObject
                    {
                        GameObjectName = gameObjectName,
                        Description = i == 0 ? viewModel.Description : null,
                        Sprite = stage.Sprite,
                        TransformName = previousGameObject?.GameObjectName
                    };

                    _context.GameObject.Add(gameObject);
                    await _context.SaveChangesAsync();

                    var entity = new Entity
                    {
                        EntityId = stage.EntityId,
                        GameObjectName = gameObject.GameObjectName,
                        Hp = stage.Hp,
                        Defense = (short)stage.Defense
                    };

                    _context.Entity.Add(entity);
                    await _context.SaveChangesAsync();

                    var townNpc = new TownNpc
                    {
                        EntityId = entity.EntityId
                    };

                    _context.TownNpc.Add(townNpc);
                    await _context.SaveChangesAsync();

                    if (i == 0)
                    {
                        foreach (var drop in viewModel.Drops.Where(d => !string.IsNullOrEmpty(d.ItemId) && d.Quantity > 0))
                        {
                            var entityDrop = new EntityDrop
                            {
                                EntityId = entity.EntityId,
                                ItemId = short.Parse(drop.ItemId),
                                Quantity = (short)drop.Quantity
                            };
                            _context.EntityDrop.Add(entityDrop);
                        }
                    }

                    if (i == 0)
                    {
                        foreach (var trade in validTrades)
                        {
                            var tradeOffer = new TradeOffer
                            {
                                TownNpcId = townNpc.TownNpcId,
                                ItemId = short.Parse(trade.ItemId),
                                TradeTypeName = trade.TradeType,
                                Quantity = (short)trade.Quantity
                            };
                            _context.TradeOffer.Add(tradeOffer);
                        }
                    }

                    previousGameObject = gameObject;
                    previousEntity = entity;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: TownNpcs/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(byte id)
        {
            var townNpc = await _context.TownNpc
                .Include(t => t.Entity)
                    .ThenInclude(e => e.GameObject)
                .FirstOrDefaultAsync(t => t.TownNpcId == id);

            if (townNpc == null)
            {
                return NotFound();
            }

            var viewModel = new TownNpcDeleteViewModel
            {
                TownNpcId = townNpc.TownNpcId.ToString(),
                Name = townNpc.Entity.GameObject.GameObjectName,
                Sprite = townNpc.Entity.GameObject.Sprite
            };

            return View(viewModel);
        }

        // POST: TownNpcs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(byte id)
        {
            var townNpc = await _context.TownNpc
                .Include(t => t.Entity)
                    .ThenInclude(e => e.GameObject)
                .Include(t => t.Entity)
                    .ThenInclude(e => e.EntityDrops)
                .Include(t => t.TradeOffers)
                .FirstOrDefaultAsync(t => t.TownNpcId == id);

            if (townNpc == null)
            {
                return NotFound();
            }

            var allGameObjects = new List<GameObject>();
            var allEntityDrops = new List<EntityDrop>();
            var allTradeOffers = new List<TradeOffer>();

            await CollectTownNpcData(townNpc.Entity.GameObject, allGameObjects, allEntityDrops, allTradeOffers);

            _context.TradeOffer.RemoveRange(allTradeOffers);
            _context.EntityDrop.RemoveRange(allEntityDrops);

            foreach (var go in allGameObjects)
            {
                _context.GameObject.Remove(go);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task CollectTownNpcData(GameObject startGameObject,
            List<GameObject> gameObjects,
            List<EntityDrop> entityDrops,
            List<TradeOffer> tradeOffers)
        {
            var current = startGameObject;

            while (current != null && !gameObjects.Contains(current))
            {
                gameObjects.Add(current);

                var townNpcAtStage = await _context.TownNpc
                    .Include(t => t.TradeOffers)
                    .Include(t => t.Entity)
                        .ThenInclude(e => e.EntityDrops)
                    .FirstOrDefaultAsync(t => t.Entity.GameObjectName == current.GameObjectName);

                if (townNpcAtStage != null)
                {
                    tradeOffers.AddRange(townNpcAtStage.TradeOffers);
                    entityDrops.AddRange(townNpcAtStage.Entity.EntityDrops);
                }

                current = await _context.GameObject
                    .FirstOrDefaultAsync(go => go.GameObjectName == current.TransformName);
            }
        }
    }
}
