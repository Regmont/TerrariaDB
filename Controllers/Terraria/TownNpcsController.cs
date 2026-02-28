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
                Transformations = await GetTransformations(townNpc.Entity.GameObject)
            };

            return View(viewModel);
        }

        private async Task<List<TownNpcTransformationViewModel>> GetTransformations(GameObject gameObject)
        {
            var transformations = new List<TownNpcTransformationViewModel>();

            var firstStageNpc = await _context.TownNpc
                .Include(t => t.Entity)
                .FirstOrDefaultAsync(t => t.Entity.GameObjectName == gameObject.GameObjectName);

            if (firstStageNpc != null)
            {
                transformations.Add(new TownNpcTransformationViewModel
                {
                    Name = gameObject.GameObjectName,
                    Sprite = gameObject.Sprite,
                    EntityId = firstStageNpc.EntityId.ToString(),
                    Hp = firstStageNpc.Entity.Hp ?? 0,
                    Defense = firstStageNpc.Entity.Defense
                });
            }

            var current = gameObject.Transform;
            while (current != null)
            {
                var npcAtStage = await _context.TownNpc
                    .Include(t => t.Entity)
                    .FirstOrDefaultAsync(t => t.Entity.GameObjectName == current.GameObjectName);

                if (npcAtStage != null)
                {
                    transformations.Add(new TownNpcTransformationViewModel
                    {
                        Name = current.GameObjectName,
                        Sprite = current.Sprite,
                        EntityId = npcAtStage.EntityId.ToString(),
                        Hp = npcAtStage.Entity.Hp ?? 0,
                        Defense = npcAtStage.Entity.Defense
                    });
                }

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

            if (string.IsNullOrEmpty(viewModel.Stages[0].Sprite))
            {
                ModelState.AddModelError("Stages[0].Sprite", "Sprite for first stage is required");
                return View(viewModel);
            }

            var filledStages = viewModel.Stages
                .Where(s => !string.IsNullOrEmpty(s.Sprite))
                .ToList();

            if (!filledStages.Any())
            {
                ModelState.AddModelError("", "At least one stage must be filled");
                return View(viewModel);
            }

            foreach (var stage in filledStages)
            {
                if (stage.Hp < 0 || stage.Hp > 30000)
                {
                    ModelState.AddModelError("", "HP must be between 0 and 30000");
                    return View(viewModel);
                }
                if (stage.Defense < 0 || stage.Defense > 10000)
                {
                    ModelState.AddModelError("", "Defense must be between 0 and 10000");
                    return View(viewModel);
                }
                if (stage.EntityId < -500 || stage.EntityId > 1000)
                {
                    ModelState.AddModelError("", "Entity ID must be between -500 and 1000");
                    return View(viewModel);
                }
            }

            var allEntityIds = filledStages.Select(s => s.EntityId).ToList();
            if (allEntityIds.Count != allEntityIds.Distinct().Count())
            {
                ModelState.AddModelError("", "Entity IDs must be unique across all stages");
                return View(viewModel);
            }

            foreach (var entityId in allEntityIds)
            {
                if (await _context.Entity.AnyAsync(e => e.EntityId == entityId))
                {
                    ModelState.AddModelError("", $"Entity ID {entityId} is already in use");
                    return View(viewModel);
                }
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

            if (ModelState.IsValid)
            {
                GameObject? previousGameObject = null;

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

                    var entity = new Entity
                    {
                        EntityId = stage.EntityId,
                        GameObjectName = gameObject.GameObjectName,
                        Hp = stage.Hp,
                        Defense = (short)stage.Defense
                    };

                    _context.Entity.Add(entity);

                    var townNpc = new TownNpc
                    {
                        EntityId = entity.EntityId
                    };

                    _context.TownNpc.Add(townNpc);

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
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: TownNpcs/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(byte id)
        {
            var townNpc = await _context.TownNpc
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
                .FirstOrDefaultAsync(tn => tn.TownNpcId == id);

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

            var stages = new List<(GameObject go, Entity entity, TownNpc npc)>();
            var current = townNpc.Entity.GameObject;
            while (current != null)
            {
                var npcAtStage = await _context.TownNpc
                    .Include(t => t.Entity)
                    .FirstOrDefaultAsync(t => t.Entity.GameObjectName == current.GameObjectName);

                if (npcAtStage != null)
                {
                    stages.Add((current, npcAtStage.Entity, npcAtStage));
                }
                current = await _context.GameObject
                    .FirstOrDefaultAsync(go => go.GameObjectName == current.TransformName);
            }

            for (int i = 0; i < 4; i++)
            {
                var stage = new TownNpcEditStageViewModel();

                if (i < stages.Count)
                {
                    var (go, entity, npc) = stages[i];
                    stage.Sprite = go.Sprite;
                    stage.Hp = entity.Hp ?? DefaultHp;
                    stage.Defense = entity.Defense;
                    stage.EntityId = entity.EntityId;
                }
                else
                {
                    stage.Hp = DefaultHp;
                    stage.Defense = DefaultDefense;
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

            if (string.IsNullOrEmpty(viewModel.Stages[0].Sprite))
            {
                ModelState.AddModelError("Stages[0].Sprite", "Sprite for first stage is required");
                return View(viewModel);
            }

            var filledStages = viewModel.Stages
                .Where(s => !string.IsNullOrEmpty(s.Sprite))
                .ToList();

            if (!filledStages.Any())
            {
                ModelState.AddModelError("", "At least one stage must be filled");
                return View(viewModel);
            }

            foreach (var stage in filledStages)
            {
                if (stage.Hp < 0 || stage.Hp > 30000)
                {
                    ModelState.AddModelError("", "HP must be between 0 and 30000");
                    return View(viewModel);
                }
                if (stage.Defense < 0 || stage.Defense > 10000)
                {
                    ModelState.AddModelError("", "Defense must be between 0 and 10000");
                    return View(viewModel);
                }
                if (stage.EntityId < -500 || stage.EntityId > 1000)
                {
                    ModelState.AddModelError("", "Entity ID must be between -500 and 1000");
                    return View(viewModel);
                }
            }

            var allEntityIds = filledStages.Select(s => s.EntityId).ToList();
            if (allEntityIds.Count != allEntityIds.Distinct().Count())
            {
                ModelState.AddModelError("", "Entity IDs must be unique across all stages");
                return View(viewModel);
            }

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

            var existingStages = new List<(GameObject go, Entity entity, TownNpc npc)>();
            var current = originalTownNpc.Entity.GameObject;
            while (current != null)
            {
                var npcAtStage = await _context.TownNpc
                    .Include(t => t.Entity)
                    .FirstOrDefaultAsync(t => t.Entity.GameObjectName == current.GameObjectName);

                if (npcAtStage != null)
                {
                    existingStages.Add((current, npcAtStage.Entity, npcAtStage));
                }
                current = await _context.GameObject
                    .FirstOrDefaultAsync(go => go.GameObjectName == current.TransformName);
            }

            var allGameObjectNames = filledStages
                .Select((s, index) => index == 0 ? viewModel.Name : $"{viewModel.Name}_{index + 1}")
                .ToList();

            var existingNames = existingStages.Select(s => s.go.GameObjectName).ToList();
            for (int i = 0; i < allGameObjectNames.Count; i++)
            {
                var name = allGameObjectNames[i];
                if (!existingNames.Contains(name) &&
                    await _context.GameObject.AnyAsync(go => go.GameObjectName == name))
                {
                    ModelState.AddModelError("", $"Game object with name '{name}' already exists");
                    return View(viewModel);
                }
            }

            var allSprites = filledStages.Select(s => s.Sprite).ToList();
            var existingSprites = existingStages.Select(s => s.go.Sprite).ToList();
            for (int i = 0; i < allSprites.Count; i++)
            {
                var sprite = allSprites[i];
                if (!existingSprites.Contains(sprite) &&
                    await _context.GameObject.AnyAsync(go => go.Sprite == sprite))
                {
                    ModelState.AddModelError("", $"Sprite '{sprite}' already exists");
                    return View(viewModel);
                }
            }

            var existingEntityIds = existingStages.Select(s => s.entity.EntityId).ToList();
            foreach (var entityId in allEntityIds)
            {
                if (!existingEntityIds.Contains(entityId) &&
                    await _context.Entity.AnyAsync(e => e.EntityId == entityId))
                {
                    ModelState.AddModelError("", $"Entity ID {entityId} is already in use");
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

            if (ModelState.IsValid)
            {
                _context.TradeOffer.RemoveRange(originalTownNpc.TradeOffers);

                GameObject? previousGameObject = null;

                for (int i = 0; i < filledStages.Count; i++)
                {
                    var stage = filledStages[i];
                    var gameObjectName = i == 0 ? viewModel.Name : $"{viewModel.Name}_{i + 1}";

                    GameObject gameObject;
                    Entity entity;
                    TownNpc townNpc;

                    if (i < existingStages.Count)
                    {
                        (gameObject, entity, townNpc) = existingStages[i];

                        gameObject.GameObjectName = gameObjectName;
                        gameObject.Description = i == 0 ? viewModel.Description : null;
                        gameObject.Sprite = stage.Sprite;
                        gameObject.TransformName = previousGameObject?.GameObjectName;

                        entity.EntityId = stage.EntityId;
                        entity.Hp = stage.Hp;
                        entity.Defense = (short)stage.Defense;

                        _context.GameObject.Update(gameObject);
                        _context.Entity.Update(entity);

                        var oldDrops = await _context.EntityDrop
                            .Where(ed => ed.EntityId == entity.EntityId)
                            .ToListAsync();
                        _context.EntityDrop.RemoveRange(oldDrops);
                    }
                    else
                    {
                        gameObject = new GameObject
                        {
                            GameObjectName = gameObjectName,
                            Description = i == 0 ? viewModel.Description : null,
                            Sprite = stage.Sprite,
                            TransformName = previousGameObject?.GameObjectName
                        };
                        _context.GameObject.Add(gameObject);

                        entity = new Entity
                        {
                            EntityId = stage.EntityId,
                            GameObjectName = gameObject.GameObjectName,
                            Hp = stage.Hp,
                            Defense = (short)stage.Defense
                        };
                        _context.Entity.Add(entity);

                        townNpc = new TownNpc
                        {
                            EntityId = entity.EntityId
                        };
                        _context.TownNpc.Add(townNpc);
                    }

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
                }

                for (int i = filledStages.Count; i < existingStages.Count; i++)
                {
                    var (go, entity, npc) = existingStages[i];

                    var drops = await _context.EntityDrop
                        .Where(ed => ed.EntityId == entity.EntityId)
                        .ToListAsync();
                    _context.EntityDrop.RemoveRange(drops);

                    _context.TownNpc.Remove(npc);
                    _context.Entity.Remove(entity);
                    _context.GameObject.Remove(go);
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
