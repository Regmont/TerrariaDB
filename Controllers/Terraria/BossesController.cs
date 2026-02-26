using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;
using TerrariaDB.ViewModels.Terraria.Boss;

namespace TerrariaDB.Controllers.Terraria
{
    public class BossesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BossesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bosses
        public async Task<IActionResult> Index()
        {
            var bosses = await _context.Boss
                .Select(b => new BossItemViewModel
                {
                    Name = b.BossName,
                    Sprite = b.BossParts
                        .Select(bp => bp.HostileEntity.Entity.GameObject)
                        .FirstOrDefault()!.Sprite
                })
                .ToListAsync();

            var viewModel = new BossIndexViewModel
            {
                Bosses = bosses
            };

            return View(viewModel);
        }

        // GET: Bosses/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var boss = await _context.Boss
                .Include(b => b.BossDrops)
                    .ThenInclude(bd => bd.Item)
                        .ThenInclude(i => i.GameObject)
                .Include(b => b.BossParts)
                    .ThenInclude(bp => bp.HostileEntity)
                        .ThenInclude(he => he.Entity)
                            .ThenInclude(e => e.GameObject)
                .Include(b => b.BossParts)
                    .ThenInclude(bp => bp.HostileEntity)
                        .ThenInclude(he => he.Entity)
                            .ThenInclude(e => e.EntityDrops)
                                .ThenInclude(ed => ed.Item)
                                    .ThenInclude(i => i.GameObject)
                .Include(b => b.BossParts)
                    .ThenInclude(bp => bp.BossPartEnemies)
                        .ThenInclude(bpe => bpe.Enemy)
                            .ThenInclude(e => e.HostileEntity)
                                .ThenInclude(he => he.Entity)
                                    .ThenInclude(en => en.GameObject)
                .FirstOrDefaultAsync(b => b.BossName == id);

            if (boss == null)
            {
                return NotFound();
            }

            var viewModel = new BossDetailsViewModel
            {
                BossName = boss.BossName,
                Drops = boss.BossDrops.Select(bd => new BossDropViewModel
                {
                    Name = bd.Item.GameObject.GameObjectName,
                    Sprite = bd.Item.GameObject.Sprite,
                    Quantity = bd.Quantity
                }).ToList(),
                BossParts = boss.BossParts.Select(bp => new BossPartViewModel
                {
                    Name = bp.HostileEntity.Entity.GameObject.GameObjectName,
                    Description = bp.HostileEntity.Entity.GameObject.Description ?? string.Empty,
                    Quantity = bp.Quantity,
                    Stages = GetStages(bp.HostileEntity.Entity.GameObject, bp)
                }).ToList()
            };

            return View(viewModel);
        }

        private List<BossStageViewModel> GetStages(GameObject gameObject, BossPart bossPart)
        {
            var stages = new List<BossStageViewModel>();

            var current = gameObject;
            while (current != null)
            {
                stages.Add(new BossStageViewModel
                {
                    Name = current.GameObjectName,
                    Sprite = current.Sprite,
                    EntityId = bossPart.HostileEntity.EntityId.ToString(),
                    Hp = bossPart.HostileEntity.Entity.Hp ?? 0,
                    Defense = bossPart.HostileEntity.Entity.Defense,
                    ContactDamage = bossPart.HostileEntity.ContactDamage,
                    SummonedEnemies = bossPart.BossPartEnemies.Select(bpe => new BossStageEnemyViewModel
                    {
                        Name = bpe.Enemy.HostileEntity.Entity.GameObject.GameObjectName,
                        Sprite = bpe.Enemy.HostileEntity.Entity.GameObject.Sprite,
                        Quantity = bpe.Quantity
                    }).ToList(),
                    Drops = bossPart.HostileEntity.Entity.EntityDrops.Select(ed => new BossStageDropViewModel
                    {
                        Name = ed.Item.GameObject.GameObjectName,
                        Sprite = ed.Item.GameObject.Sprite,
                        Quantity = ed.Quantity
                    }).ToList()
                });

                current = current.Transform;
            }

            return stages;
        }

        // GET: Bosses/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var viewModel = new BossCreateViewModel();

            viewModel.AvailableItems = _context.Item
                .Include(i => i.GameObject)
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.ItemId.ToString(),
                    Text = i.GameObject.GameObjectName
                })
                .ToList();

            viewModel.AvailableEnemies = _context.Enemy
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                        .ThenInclude(e => e.GameObject)
                .Where(e => e.HostileEntity.Entity.GameObject.TransformedFrom == null)
                .Select(e => new SelectListItem
                {
                    Value = e.EnemyId.ToString(),
                    Text = e.HostileEntity.Entity.GameObject.GameObjectName
                })
                .ToList();

            for (int i = 0; i < 15; i++)
            {
                viewModel.BossDrops.Add(new BossDropCreateViewModel());
            }

            for (int i = 0; i < 5; i++)
            {
                var part = new BossPartCreateViewModel();

                for (int j = 0; j < 2; j++)
                {
                    var stage = new BossStageCreateViewModel();

                    for (int k = 0; k < 3; k++)
                    {
                        stage.SpawnedEnemies.Add(new BossStageEnemyCreateViewModel());
                        stage.Drops.Add(new BossStageDropCreateViewModel());
                    }

                    part.Stages.Add(stage);
                }

                viewModel.BossParts.Add(part);
            }

            return View(viewModel);
        }

        // POST: Bosses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(BossCreateViewModel viewModel)
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

            viewModel.AvailableEnemies = _context.Enemy
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                        .ThenInclude(e => e.GameObject)
                .Where(e => e.HostileEntity.Entity.GameObject.TransformedFrom == null)
                .Select(e => new SelectListItem
                {
                    Value = e.EnemyId.ToString(),
                    Text = e.HostileEntity.Entity.GameObject.GameObjectName
                })
                .ToList();

            if (ModelState.IsValid)
            {
                if (await _context.Boss.AnyAsync(b => b.BossName == viewModel.BossName))
                {
                    ModelState.AddModelError("BossName", "A boss with this name already exists");
                    return View(viewModel);
                }

                var filledParts = viewModel.BossParts
                    .Where(p => !string.IsNullOrEmpty(p.PartName))
                    .ToList();

                if (!filledParts.Any())
                {
                    ModelState.AddModelError("", "At least one boss part must be filled");
                    return View(viewModel);
                }

                var allGameObjectNames = new List<string>();
                var allSprites = new List<string>();
                var allEntityIds = new List<int>();

                foreach (var part in filledParts)
                {
                    var filledStages = part.Stages
                        .Where(s => !string.IsNullOrEmpty(s.Sprite))
                        .ToList();

                    if (!filledStages.Any())
                    {
                        ModelState.AddModelError("", $"Part '{part.PartName}' must have at least one stage");
                        return View(viewModel);
                    }

                    for (int i = 0; i < filledStages.Count; i++)
                    {
                        var stage = filledStages[i];
                        var gameObjectName = i == 0 ? part.PartName : $"{part.PartName}_{i + 1}";

                        allGameObjectNames.Add(gameObjectName);
                        allSprites.Add(stage.Sprite);
                        allEntityIds.Add(stage.EntityId);
                    }
                }

                if (allGameObjectNames.Count != allGameObjectNames.Distinct().Count())
                {
                    ModelState.AddModelError("", "Game object names must be unique across all parts and stages");
                    return View(viewModel);
                }

                if (allSprites.Count != allSprites.Distinct().Count())
                {
                    ModelState.AddModelError("", "Sprites must be unique across all parts and stages");
                    return View(viewModel);
                }

                if (allEntityIds.Count != allEntityIds.Distinct().Count())
                {
                    ModelState.AddModelError("", "Entity IDs must be unique across all parts and stages");
                    return View(viewModel);
                }

                foreach (var name in allGameObjectNames)
                {
                    if (await _context.GameObject.AnyAsync(go => go.GameObjectName == name))
                    {
                        ModelState.AddModelError("", $"Game object with name '{name}' already exists");
                        return View(viewModel);
                    }
                }

                foreach (var sprite in allSprites)
                {
                    if (await _context.GameObject.AnyAsync(go => go.Sprite == sprite))
                    {
                        ModelState.AddModelError("", $"Sprite '{sprite}' already exists");
                        return View(viewModel);
                    }
                }

                var boss = new Boss
                {
                    BossName = viewModel.BossName,
                    SummonItemId = !string.IsNullOrEmpty(viewModel.SummonItemId) ? short.Parse(viewModel.SummonItemId) : null
                };

                _context.Boss.Add(boss);
                await _context.SaveChangesAsync();

                foreach (var drop in viewModel.BossDrops.Where(d => !string.IsNullOrEmpty(d.ItemId) && d.Quantity > 0))
                {
                    var bossDrop = new BossDrop
                    {
                        BossName = boss.BossName,
                        ItemId = short.Parse(drop.ItemId),
                        Quantity = (short)drop.Quantity
                    };
                    _context.BossDrop.Add(bossDrop);
                }

                foreach (var part in filledParts)
                {
                    var filledStages = part.Stages
                        .Where(s => !string.IsNullOrEmpty(s.Sprite))
                        .ToList();

                    GameObject? previousGameObject = null;
                    Entity? previousEntity = null;

                    for (int i = 0; i < filledStages.Count; i++)
                    {
                        var stage = filledStages[i];
                        var gameObjectName = i == 0 ? part.PartName : $"{part.PartName}_{i + 1}";

                        var gameObject = new GameObject
                        {
                            GameObjectName = gameObjectName,
                            Description = i == 0 ? part.Description : null,
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

                        var hostileEntity = new HostileEntity
                        {
                            EntityId = entity.EntityId,
                            ContactDamage = (short)stage.ContactDamage
                        };

                        _context.HostileEntity.Add(hostileEntity);
                        await _context.SaveChangesAsync();

                        var bossPart = new BossPart
                        {
                            BossName = boss.BossName,
                            HostileEntityId = hostileEntity.HostileEntityId,
                            Quantity = (short)part.Quantity
                        };

                        _context.BossPart.Add(bossPart);
                        await _context.SaveChangesAsync();

                        if (i == 0)
                        {
                            foreach (var enemy in stage.SpawnedEnemies.Where(e => !string.IsNullOrEmpty(e.EnemyId) && e.Quantity > 0))
                            {
                                var bossPartEnemy = new BossPartEnemies
                                {
                                    BossPartId = bossPart.BossPartId,
                                    EnemyId = short.Parse(enemy.EnemyId),
                                    Quantity = (short)enemy.Quantity
                                };
                                _context.BossPartEnemies.Add(bossPartEnemy);
                            }
                        }

                        foreach (var drop in stage.Drops.Where(d => !string.IsNullOrEmpty(d.ItemId) && d.Quantity > 0))
                        {
                            var entityDrop = new EntityDrop
                            {
                                EntityId = entity.EntityId,
                                ItemId = short.Parse(drop.ItemId),
                                Quantity = (short)drop.Quantity
                            };
                            _context.EntityDrop.Add(entityDrop);
                        }

                        previousGameObject = gameObject;
                        previousEntity = entity;
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Bosses/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(string name)
        {
            var boss = _context.Boss
                .Include(b => b.BossParts)
                    .ThenInclude(bp => bp.HostileEntity)
                        .ThenInclude(he => he.Entity)
                            .ThenInclude(e => e.GameObject)
                .Include(b => b.BossParts)
                    .ThenInclude(bp => bp.HostileEntity)
                        .ThenInclude(he => he.Entity)
                            .ThenInclude(e => e.EntityDrops)
                                .ThenInclude(ed => ed.Item)
                                    .ThenInclude(i => i.GameObject)
                .Include(b => b.BossParts)
                    .ThenInclude(bp => bp.BossPartEnemies)
                        .ThenInclude(bpe => bpe.Enemy)
                            .ThenInclude(e => e.HostileEntity)
                                .ThenInclude(he => he.Entity)
                                    .ThenInclude(e => e.GameObject)
                .Include(b => b.BossDrops)
                    .ThenInclude(bd => bd.Item)
                        .ThenInclude(i => i.GameObject)
                .FirstOrDefault(b => b.BossName == name);

            if (boss == null)
            {
                return NotFound();
            }

            var viewModel = new BossEditViewModel
            {
                OriginalBossName = boss.BossName,
                BossName = boss.BossName,
                SummonItemId = boss.SummonItemId?.ToString()
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

            viewModel.AvailableEnemies = _context.Enemy
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                        .ThenInclude(e => e.GameObject)
                .Where(e => e.HostileEntity.Entity.GameObject.TransformedFrom == null)
                .Select(e => new SelectListItem
                {
                    Value = e.EnemyId.ToString(),
                    Text = e.HostileEntity.Entity.GameObject.GameObjectName
                })
                .ToList();

            var bossDrops = boss.BossDrops.ToList();
            for (int i = 0; i < 15; i++)
            {
                var drop = new BossDropEditViewModel();
                if (i < bossDrops.Count)
                {
                    drop.ItemId = bossDrops[i].ItemId.ToString();
                    drop.Quantity = bossDrops[i].Quantity;
                }
                viewModel.BossDrops.Add(drop);
            }

            var bossParts = boss.BossParts.ToList();
            for (int i = 0; i < 5; i++)
            {
                var part = new BossPartEditViewModel();

                if (i < bossParts.Count)
                {
                    var currentPart = bossParts[i];
                    part.PartName = currentPart.HostileEntity.Entity.GameObject.GameObjectName;
                    part.Description = currentPart.HostileEntity.Entity.GameObject.Description ?? string.Empty;
                    part.Quantity = currentPart.Quantity;

                    for (int j = 0; j < 2; j++)
                    {
                        var stage = new BossStageEditViewModel();

                        if (j == 0)
                        {
                            stage.Sprite = currentPart.HostileEntity.Entity.GameObject.Sprite;
                            stage.Hp = currentPart.HostileEntity.Entity.Hp ?? 0;
                            stage.Defense = currentPart.HostileEntity.Entity.Defense;
                            stage.EntityId = currentPart.HostileEntity.Entity.EntityId;
                            stage.ContactDamage = currentPart.HostileEntity.ContactDamage;

                            var spawnedEnemies = currentPart.BossPartEnemies.ToList();
                            for (int k = 0; k < 3; k++)
                            {
                                var enemy = new BossStageEnemyEditViewModel();
                                if (k < spawnedEnemies.Count)
                                {
                                    enemy.EnemyId = spawnedEnemies[k].EnemyId.ToString();
                                    enemy.Quantity = spawnedEnemies[k].Quantity;
                                }
                                stage.SpawnedEnemies.Add(enemy);
                            }

                            var drops = currentPart.HostileEntity.Entity.EntityDrops.ToList();
                            for (int k = 0; k < 3; k++)
                            {
                                var drop = new BossStageDropEditViewModel();
                                if (k < drops.Count)
                                {
                                    drop.ItemId = drops[k].ItemId.ToString();
                                    drop.Quantity = drops[k].Quantity;
                                }
                                stage.Drops.Add(drop);
                            }
                        }
                        else
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                stage.SpawnedEnemies.Add(new BossStageEnemyEditViewModel());
                                stage.Drops.Add(new BossStageDropEditViewModel());
                            }
                        }

                        part.Stages.Add(stage);
                    }
                }
                else
                {
                    for (int j = 0; j < 2; j++)
                    {
                        var stage = new BossStageEditViewModel();
                        for (int k = 0; k < 3; k++)
                        {
                            stage.SpawnedEnemies.Add(new BossStageEnemyEditViewModel());
                            stage.Drops.Add(new BossStageDropEditViewModel());
                        }
                        part.Stages.Add(stage);
                    }
                }

                viewModel.BossParts.Add(part);
            }

            return View(viewModel);
        }

        // POST: Bosses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(BossEditViewModel viewModel)
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

            viewModel.AvailableEnemies = _context.Enemy
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                        .ThenInclude(e => e.GameObject)
                .Where(e => e.HostileEntity.Entity.GameObject.TransformedFrom == null)
                .Select(e => new SelectListItem
                {
                    Value = e.EnemyId.ToString(),
                    Text = e.HostileEntity.Entity.GameObject.GameObjectName
                })
                .ToList();

            if (ModelState.IsValid)
            {
                var originalBoss = await _context.Boss
                    .Include(b => b.BossDrops)
                    .Include(b => b.BossParts)
                        .ThenInclude(bp => bp.HostileEntity)
                            .ThenInclude(he => he.Entity)
                                .ThenInclude(e => e.GameObject)
                    .Include(b => b.BossParts)
                        .ThenInclude(bp => bp.HostileEntity)
                            .ThenInclude(he => he.Entity)
                                .ThenInclude(e => e.EntityDrops)
                    .Include(b => b.BossParts)
                        .ThenInclude(bp => bp.BossPartEnemies)
                    .FirstOrDefaultAsync(b => b.BossName == viewModel.OriginalBossName);

                if (originalBoss == null)
                {
                    return NotFound();
                }

                if (viewModel.OriginalBossName != viewModel.BossName &&
                    await _context.Boss.AnyAsync(b => b.BossName == viewModel.BossName))
                {
                    ModelState.AddModelError("BossName", "A boss with this name already exists");
                    return View(viewModel);
                }

                var existingGameObjects = new List<GameObject>();
                var existingEntities = new List<Entity>();
                var existingHostileEntities = new List<HostileEntity>();
                var existingBossParts = new List<BossPart>();
                var existingBossPartEnemies = new List<BossPartEnemies>();
                var existingEntityDrops = new List<EntityDrop>();

                foreach (var part in originalBoss.BossParts)
                {
                    var current = part.HostileEntity.Entity.GameObject;
                    while (current != null && !existingGameObjects.Contains(current))
                    {
                        existingGameObjects.Add(current);
                        var entity = await _context.Entity
                            .Include(e => e.EntityDrops)
                            .FirstOrDefaultAsync(e => e.GameObjectName == current.GameObjectName);
                        if (entity != null)
                        {
                            existingEntities.Add(entity);
                            existingEntityDrops.AddRange(entity.EntityDrops);
                        }
                        current = await _context.GameObject
                            .FirstOrDefaultAsync(go => go.GameObjectName == current.TransformName);
                    }
                    existingHostileEntities.Add(part.HostileEntity);
                    existingBossParts.Add(part);
                    existingBossPartEnemies.AddRange(part.BossPartEnemies);
                }

                var filledParts = viewModel.BossParts
                    .Where(p => !string.IsNullOrEmpty(p.PartName))
                    .ToList();

                if (!filledParts.Any())
                {
                    ModelState.AddModelError("", "At least one boss part must be filled");
                    return View(viewModel);
                }

                var allGameObjectNames = new List<string>();
                var allSprites = new List<string>();
                var allEntityIds = new List<int>();

                foreach (var part in filledParts)
                {
                    var filledStages = part.Stages
                        .Where(s => !string.IsNullOrEmpty(s.Sprite))
                        .ToList();

                    if (!filledStages.Any())
                    {
                        ModelState.AddModelError("", $"Part '{part.PartName}' must have at least one stage");
                        return View(viewModel);
                    }

                    for (int i = 0; i < filledStages.Count; i++)
                    {
                        var stage = filledStages[i];
                        var gameObjectName = i == 0 ? part.PartName : $"{part.PartName}_{i + 1}";

                        allGameObjectNames.Add(gameObjectName);
                        allSprites.Add(stage.Sprite);
                        allEntityIds.Add(stage.EntityId);
                    }
                }

                if (allGameObjectNames.Count != allGameObjectNames.Distinct().Count())
                {
                    ModelState.AddModelError("", "Game object names must be unique across all parts and stages");
                    return View(viewModel);
                }

                if (allSprites.Count != allSprites.Distinct().Count())
                {
                    ModelState.AddModelError("", "Sprites must be unique across all parts and stages");
                    return View(viewModel);
                }

                if (allEntityIds.Count != allEntityIds.Distinct().Count())
                {
                    ModelState.AddModelError("", "Entity IDs must be unique across all parts and stages");
                    return View(viewModel);
                }

                foreach (var name in allGameObjectNames)
                {
                    if (!existingGameObjects.Any(go => go.GameObjectName == name) &&
                        await _context.GameObject.AnyAsync(go => go.GameObjectName == name))
                    {
                        ModelState.AddModelError("", $"Game object with name '{name}' already exists");
                        return View(viewModel);
                    }
                }

                foreach (var sprite in allSprites)
                {
                    if (!existingGameObjects.Any(go => go.Sprite == sprite) &&
                        await _context.GameObject.AnyAsync(go => go.Sprite == sprite))
                    {
                        ModelState.AddModelError("", $"Sprite '{sprite}' already exists");
                        return View(viewModel);
                    }
                }

                _context.BossDrop.RemoveRange(originalBoss.BossDrops);
                _context.BossPartEnemies.RemoveRange(existingBossPartEnemies);
                _context.EntityDrop.RemoveRange(existingEntityDrops);
                _context.BossPart.RemoveRange(existingBossParts);
                _context.HostileEntity.RemoveRange(existingHostileEntities);
                _context.Entity.RemoveRange(existingEntities);
                _context.GameObject.RemoveRange(existingGameObjects);
                _context.Boss.Remove(originalBoss);
                await _context.SaveChangesAsync();

                var boss = new Boss
                {
                    BossName = viewModel.BossName,
                    SummonItemId = !string.IsNullOrEmpty(viewModel.SummonItemId) ? short.Parse(viewModel.SummonItemId) : null
                };

                _context.Boss.Add(boss);
                await _context.SaveChangesAsync();

                foreach (var drop in viewModel.BossDrops.Where(d => !string.IsNullOrEmpty(d.ItemId) && d.Quantity > 0))
                {
                    var bossDrop = new BossDrop
                    {
                        BossName = boss.BossName,
                        ItemId = short.Parse(drop.ItemId),
                        Quantity = (short)drop.Quantity
                    };
                    _context.BossDrop.Add(bossDrop);
                }

                foreach (var part in filledParts)
                {
                    var filledStages = part.Stages
                        .Where(s => !string.IsNullOrEmpty(s.Sprite))
                        .ToList();

                    GameObject? previousGameObject = null;
                    Entity? previousEntity = null;

                    for (int i = 0; i < filledStages.Count; i++)
                    {
                        var stage = filledStages[i];
                        var gameObjectName = i == 0 ? part.PartName : $"{part.PartName}_{i + 1}";

                        var gameObject = new GameObject
                        {
                            GameObjectName = gameObjectName,
                            Description = i == 0 ? part.Description : null,
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

                        var hostileEntity = new HostileEntity
                        {
                            EntityId = entity.EntityId,
                            ContactDamage = (short)stage.ContactDamage
                        };

                        _context.HostileEntity.Add(hostileEntity);
                        await _context.SaveChangesAsync();

                        var bossPart = new BossPart
                        {
                            BossName = boss.BossName,
                            HostileEntityId = hostileEntity.HostileEntityId,
                            Quantity = (short)part.Quantity
                        };

                        _context.BossPart.Add(bossPart);
                        await _context.SaveChangesAsync();

                        if (i == 0)
                        {
                            foreach (var enemy in stage.SpawnedEnemies.Where(e => !string.IsNullOrEmpty(e.EnemyId) && e.Quantity > 0))
                            {
                                var bossPartEnemy = new BossPartEnemies
                                {
                                    BossPartId = bossPart.BossPartId,
                                    EnemyId = short.Parse(enemy.EnemyId),
                                    Quantity = (short)enemy.Quantity
                                };
                                _context.BossPartEnemies.Add(bossPartEnemy);
                            }
                        }

                        foreach (var drop in stage.Drops.Where(d => !string.IsNullOrEmpty(d.ItemId) && d.Quantity > 0))
                        {
                            var entityDrop = new EntityDrop
                            {
                                EntityId = entity.EntityId,
                                ItemId = short.Parse(drop.ItemId),
                                Quantity = (short)drop.Quantity
                            };
                            _context.EntityDrop.Add(entityDrop);
                        }

                        previousGameObject = gameObject;
                        previousEntity = entity;
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Bosses/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var boss = await _context.Boss
                .Include(b => b.BossParts)
                    .ThenInclude(bp => bp.HostileEntity)
                        .ThenInclude(he => he.Entity)
                            .ThenInclude(e => e.GameObject)
                .FirstOrDefaultAsync(b => b.BossName == id);

            if (boss == null)
            {
                return NotFound();
            }

            var viewModel = new BossDeleteViewModel
            {
                BossName = boss.BossName,
                Sprite = boss.BossParts
                    .Select(bp => bp.HostileEntity.Entity.GameObject)
                    .FirstOrDefault()?.Sprite ?? string.Empty
            };

            return View(viewModel);
        }

        // POST: Bosses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(BossDeleteViewModel viewModel)
        {
            var boss = await _context.Boss
                .Include(b => b.BossDrops)
                .Include(b => b.BossParts)
                    .ThenInclude(bp => bp.BossPartEnemies)
                .Include(b => b.BossParts)
                    .ThenInclude(bp => bp.HostileEntity)
                        .ThenInclude(he => he.Entity)
                            .ThenInclude(e => e.EntityDrops)
                .Include(b => b.BossParts)
                    .ThenInclude(bp => bp.HostileEntity)
                        .ThenInclude(he => he.Entity)
                            .ThenInclude(e => e.GameObject)
                .FirstOrDefaultAsync(b => b.BossName == viewModel.BossName);

            if (boss == null)
            {
                return NotFound();
            }

            _context.BossDrop.RemoveRange(boss.BossDrops);

            var allBossPartEnemies = new List<BossPartEnemies>();
            var allEntityDrops = new List<EntityDrop>();
            var allGameObjects = new List<GameObject>();

            foreach (var part in boss.BossParts)
            {
                await CollectStageData(part, allBossPartEnemies, allEntityDrops, allGameObjects);
            }

            _context.BossPartEnemies.RemoveRange(allBossPartEnemies);
            _context.EntityDrop.RemoveRange(allEntityDrops);

            foreach (var gameObject in allGameObjects)
            {
                _context.GameObject.Remove(gameObject);
            }

            _context.Boss.Remove(boss);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task CollectStageData(BossPart part,
            List<BossPartEnemies> bossPartEnemies,
            List<EntityDrop> entityDrops,
            List<GameObject> gameObjects)
        {
            bossPartEnemies.AddRange(part.BossPartEnemies);
            entityDrops.AddRange(part.HostileEntity.Entity.EntityDrops);

            var current = part.HostileEntity.Entity.GameObject;
            while (current != null && !gameObjects.Contains(current))
            {
                gameObjects.Add(current);

                var nextGameObject = await _context.GameObject
                    .Include(go => go.Entity)
                        .ThenInclude(e => e.EntityDrops)
                    .FirstOrDefaultAsync(go => go.GameObjectName == current.TransformName);

                if (nextGameObject?.Entity != null)
                {
                    var nextBossPartEnemies = await _context.BossPartEnemies
                        .Where(bpe => bpe.Enemy.HostileEntity.EntityId == nextGameObject.Entity.EntityId)
                        .ToListAsync();

                    bossPartEnemies.AddRange(nextBossPartEnemies);
                    entityDrops.AddRange(nextGameObject.Entity.EntityDrops);
                }

                current = nextGameObject;
            }
        }
    }
}
