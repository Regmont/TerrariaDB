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
                .Include(b => b.BossParts)
                    .ThenInclude(bp => bp.HostileEntity)
                        .ThenInclude(he => he.Entity)
                            .ThenInclude(e => e.GameObject)
                                .ThenInclude(g => g.TransformedFrom)
                .ToListAsync();

            var bossViewModels = bosses.Select(b =>
            {
                var firstPart = b.BossParts.FirstOrDefault();
                if (firstPart?.HostileEntity?.Entity?.GameObject == null)
                {
                    return new BossItemViewModel
                    {
                        Name = b.BossName,
                        Sprite = string.Empty
                    };
                }

                var rootGameObject = GetRootStage(firstPart.HostileEntity.Entity.GameObject);

                return new BossItemViewModel
                {
                    Name = b.BossName,
                    Sprite = rootGameObject?.Sprite ?? string.Empty
                };
            }).ToList();

            var viewModel = new BossIndexViewModel
            {
                Bosses = bossViewModels
            };

            return View(viewModel);
        }

        private GameObject? GetRootStage(GameObject? gameObject)
        {
            if (gameObject == null) return null;

            var current = gameObject;
            while (current.TransformedFrom != null)
            {
                current = current.TransformedFrom;
            }
            return current;
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
                BossParts = new List<BossPartViewModel>()
            };

            var groupedParts = boss.BossParts
                .GroupBy(bp => {
                    var fullName = bp.HostileEntity.Entity.GameObject.GameObjectName;
                    if (fullName.Contains("_"))
                        return fullName.Substring(0, fullName.IndexOf('_'));
                    return fullName;
                })
                .ToList();

            foreach (var group in groupedParts)
            {
                var partName = group.Key;
                var stages = new List<BossStageViewModel>();

                var sortedStages = group.OrderBy(bp => bp.HostileEntity.Entity.GameObject.GameObjectName).ToList();

                foreach (var stage in sortedStages)
                {
                    var gameObject = stage.HostileEntity.Entity.GameObject;
                    var entity = stage.HostileEntity.Entity;

                    var summonedEnemies = stage.BossPartEnemies.Select(bpe => new BossStageEnemyViewModel
                    {
                        Name = bpe.Enemy.HostileEntity.Entity.GameObject.GameObjectName,
                        Sprite = bpe.Enemy.HostileEntity.Entity.GameObject.Sprite,
                        Quantity = bpe.Quantity
                    }).ToList();

                    var drops = entity.EntityDrops.Select(ed => new BossStageDropViewModel
                    {
                        Name = ed.Item.GameObject.GameObjectName,
                        Sprite = ed.Item.GameObject.Sprite,
                        Quantity = ed.Quantity
                    }).ToList();

                    stages.Add(new BossStageViewModel
                    {
                        Name = gameObject.GameObjectName,
                        Sprite = gameObject.Sprite,
                        EntityId = entity.EntityId.ToString(),
                        Hp = entity.Hp ?? 0,
                        Defense = entity.Defense,
                        ContactDamage = stage.HostileEntity.ContactDamage,
                        SummonedEnemies = summonedEnemies,
                        Drops = drops
                    });
                }

                viewModel.BossParts.Add(new BossPartViewModel
                {
                    Name = partName,
                    Description = group.First().HostileEntity.Entity.GameObject.Description ?? string.Empty,
                    Quantity = group.First().Quantity,
                    Stages = stages
                });
            }

            return View(viewModel);
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

            for (int i = 0; i < viewModel.BossDrops.Count; i++)
            {
                var drop = viewModel.BossDrops[i];

                if (string.IsNullOrEmpty(drop.ItemId) || drop.Quantity <= 0)
                {
                    ModelState.Remove($"BossDrops[{i}].ItemId");
                    ModelState.Remove($"BossDrops[{i}].Quantity");
                }
            }

            for (int i = 0; i < viewModel.BossParts.Count; i++)
            {
                if (string.IsNullOrEmpty(viewModel.BossParts[i].PartName))
                {
                    ModelState.Remove($"BossParts[{i}].PartName");
                }
            }

            if (string.IsNullOrEmpty(viewModel.BossParts[0].Stages[1].Sprite))
            {
                ModelState.Remove($"BossParts[0].Stages[1].Sprite");
                ModelState.Remove($"BossParts[0].Stages[1].EntityId");
            }

            for (int i = 1; i < viewModel.BossParts.Count; i++)
            {
                for (int j = 0; j < viewModel.BossParts[i].Stages.Count; j++)
                {
                    if (string.IsNullOrEmpty(viewModel.BossParts[i].Stages[j].Sprite))
                    {
                        ModelState.Remove($"BossParts[{i}].Stages[{j}].Sprite");
                    }
                }
            }

            for (int i = 0; i < viewModel.BossParts.Count; i++)
            {
                for (int j = 0; j < viewModel.BossParts[i].Stages.Count; j++)
                {
                    for (int k = 0; k < viewModel.BossParts[i].Stages[j].Drops.Count; k++)
                    {
                        var drop = viewModel.BossParts[i].Stages[j].Drops[k];

                        if (string.IsNullOrEmpty(drop.ItemId) || drop.Quantity <= 0)
                        {
                            ModelState.Remove($"BossParts[{i}].Stages[{j}].Drops[{k}].ItemId");
                            ModelState.Remove($"BossParts[{i}].Stages[{j}].Drops[{k}].Quantity");
                        }
                    }
                }
            }

            for (int i = 0; i < viewModel.BossParts.Count; i++)
            {
                for (int j = 0; j < viewModel.BossParts[i].Stages.Count; j++)
                {
                    for (int k = 0; k < viewModel.BossParts[i].Stages[j].SpawnedEnemies.Count; k++)
                    {
                        var enemy = viewModel.BossParts[i].Stages[j].SpawnedEnemies[k];

                        if (string.IsNullOrEmpty(enemy.EnemyId) || enemy.Quantity <= 0)
                        {
                            ModelState.Remove($"BossParts[{i}].Stages[{j}].SpawnedEnemies[{k}].EnemyId");
                            ModelState.Remove($"BossParts[{i}].Stages[{j}].SpawnedEnemies[{k}].Quantity");
                        }
                    }
                }
            }

            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                if (modelStateVal?.Errors.Count > 0)
                {
                    foreach (var error in modelStateVal.Errors)
                    {
                        Console.WriteLine($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(viewModel.BossName))
                {
                    ModelState.AddModelError("BossName", "Boss name is required");
                    return View(viewModel);
                }

                if (viewModel.BossName.Length > 50)
                {
                    ModelState.AddModelError("BossName", "Boss name cannot exceed 50 characters");
                    return View(viewModel);
                }

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

                var partNames = filledParts.Select(p => p.PartName).ToList();
                if (partNames.Count != partNames.Distinct().Count())
                {
                    ModelState.AddModelError("", "Part names must be unique");
                    return View(viewModel);
                }

                var validBossDrops = viewModel.BossDrops
                    .Where(d => !string.IsNullOrEmpty(d.ItemId) && d.Quantity > 0)
                    .ToList();

                var duplicateBossDrops = validBossDrops
                    .GroupBy(d => d.ItemId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateBossDrops.Any())
                {
                    var duplicateItemNames = await _context.Item
                        .Include(i => i.GameObject)
                        .Where(i => duplicateBossDrops.Select(id => short.Parse(id)).Contains(i.ItemId))
                        .Select(i => i.GameObject.GameObjectName)
                        .ToListAsync();

                    foreach (var itemName in duplicateItemNames)
                    {
                        ModelState.AddModelError("", $"Boss drop: Item '{itemName}' appears multiple times");
                    }
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

                    foreach (var stage in filledStages)
                    {
                        if (stage.Hp < 0 || stage.Hp > 150000)
                        {
                            ModelState.AddModelError("", "HP must be between 0 and 150000");
                            return View(viewModel);
                        }
                        if (stage.Defense < 0 || stage.Defense > 100)
                        {
                            ModelState.AddModelError("", "Defense must be between 0 and 100");
                            return View(viewModel);
                        }
                        if (stage.ContactDamage < 0 || stage.ContactDamage > 500)
                        {
                            ModelState.AddModelError("", "Contact damage must be between 0 and 500");
                            return View(viewModel);
                        }
                        if (stage.EntityId < -500 || stage.EntityId > 1000)
                        {
                            ModelState.AddModelError("", "Entity ID must be between -500 and 1000");
                            return View(viewModel);
                        }
                    }

                    for (int stageIndex = 0; stageIndex < filledStages.Count; stageIndex++)
                    {
                        var stage = filledStages[stageIndex];
                        var validStageDrops = stage.Drops
                            .Where(d => !string.IsNullOrEmpty(d.ItemId) && d.Quantity > 0)
                            .ToList();

                        var duplicateStageDrops = validStageDrops
                            .GroupBy(d => d.ItemId)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();

                        if (duplicateStageDrops.Any())
                        {
                            var duplicateItemNames = await _context.Item
                                .Include(i => i.GameObject)
                                .Where(i => duplicateStageDrops.Select(id => short.Parse(id)).Contains(i.ItemId))
                                .Select(i => i.GameObject.GameObjectName)
                                .ToListAsync();

                            foreach (var itemName in duplicateItemNames)
                            {
                                ModelState.AddModelError("", $"Part '{part.PartName}', Stage {stageIndex + 1}: Item '{itemName}' appears multiple times in drops");
                            }
                            return View(viewModel);
                        }
                    }

                    for (int stageIndex = 0; stageIndex < filledStages.Count; stageIndex++)
                    {
                        var stage = filledStages[stageIndex];
                        var validSpawnedEnemies = stage.SpawnedEnemies
                            .Where(e => !string.IsNullOrEmpty(e.EnemyId) && e.Quantity > 0)
                            .ToList();

                        var duplicateSpawnedEnemies = validSpawnedEnemies
                            .GroupBy(e => e.EnemyId)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();

                        if (duplicateSpawnedEnemies.Any())
                        {
                            var duplicateEnemyNames = await _context.Enemy
                                .Include(e => e.HostileEntity)
                                    .ThenInclude(he => he.Entity)
                                        .ThenInclude(en => en.GameObject)
                                .Where(e => duplicateSpawnedEnemies.Select(id => short.Parse(id)).Contains(e.EnemyId))
                                .Select(e => e.HostileEntity.Entity.GameObject.GameObjectName)
                                .ToListAsync();

                            foreach (var enemyName in duplicateEnemyNames)
                            {
                                ModelState.AddModelError("", $"Part '{part.PartName}', Stage {stageIndex + 1}: Enemy '{enemyName}' appears multiple times in spawned enemies");
                            }
                            return View(viewModel);
                        }
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

                foreach (var entityId in allEntityIds)
                {
                    if (await _context.Entity.AnyAsync(e => e.EntityId == entityId))
                    {
                        ModelState.AddModelError("", $"Entity ID {entityId} is already in use");
                        return View(viewModel);
                    }
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

                foreach (var drop in validBossDrops)
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

                    for (int i = 0; i < filledStages.Count; i++)
                    {
                        var stage = filledStages[i];
                        var gameObjectName = i == 0 ? part.PartName : $"{part.PartName}_{i + 1}";

                        var gameObject = new GameObject
                        {
                            GameObjectName = gameObjectName,
                            Description = i == 0 ? part.Description : null,
                            Sprite = stage.Sprite,
                            TransformName = i < filledStages.Count - 1 ? $"{part.PartName}_{i + 2}" : null
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

                        var hostileEntity = new HostileEntity
                        {
                            EntityId = entity.EntityId,
                            ContactDamage = (short)stage.ContactDamage
                        };

                        _context.HostileEntity.Add(hostileEntity);

                        var bossPart = new BossPart
                        {
                            BossName = boss.BossName,
                            HostileEntity = hostileEntity,
                            Quantity = (short)part.Quantity
                        };

                        _context.BossPart.Add(bossPart);

                        foreach (var enemy in stage.SpawnedEnemies.Where(e => !string.IsNullOrEmpty(e.EnemyId) && e.Quantity > 0))
                        {
                            var bossPartEnemy = new BossPartEnemies
                            {
                                BossPart = bossPart,
                                EnemyId = short.Parse(enemy.EnemyId),
                                Quantity = (short)enemy.Quantity
                            };
                            _context.BossPartEnemies.Add(bossPartEnemy);
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
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        //// GET: Bosses/Edit/5
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Edit(string name)
        //{
        //    var boss = await _context.Boss
        //        .Include(b => b.BossParts)
        //            .ThenInclude(bp => bp.HostileEntity)
        //                .ThenInclude(he => he.Entity)
        //                    .ThenInclude(e => e.GameObject)
        //        .Include(b => b.BossParts)
        //            .ThenInclude(bp => bp.HostileEntity)
        //                .ThenInclude(he => he.Entity)
        //                    .ThenInclude(e => e.EntityDrops)
        //                        .ThenInclude(ed => ed.Item)
        //                            .ThenInclude(i => i.GameObject)
        //        .Include(b => b.BossParts)
        //            .ThenInclude(bp => bp.BossPartEnemies)
        //                .ThenInclude(bpe => bpe.Enemy)
        //                    .ThenInclude(e => e.HostileEntity)
        //                        .ThenInclude(he => he.Entity)
        //                            .ThenInclude(e => e.GameObject)
        //        .Include(b => b.BossDrops)
        //            .ThenInclude(bd => bd.Item)
        //                .ThenInclude(i => i.GameObject)
        //        .FirstOrDefaultAsync(b => b.BossName == name);

        //    if (boss == null)
        //    {
        //        return NotFound();
        //    }

        //    var viewModel = new BossEditViewModel
        //    {
        //        OriginalBossName = boss.BossName,
        //        BossName = boss.BossName,
        //        SummonItemId = boss.SummonItemId?.ToString()
        //    };

        //    viewModel.AvailableItems = _context.Item
        //        .Include(i => i.GameObject)
        //        .Where(i => i.GameObject.TransformedFrom == null)
        //        .Select(i => new SelectListItem
        //        {
        //            Value = i.ItemId.ToString(),
        //            Text = i.GameObject.GameObjectName
        //        })
        //        .ToList();

        //    viewModel.AvailableEnemies = _context.Enemy
        //        .Include(e => e.HostileEntity)
        //            .ThenInclude(he => he.Entity)
        //                .ThenInclude(e => e.GameObject)
        //        .Where(e => e.HostileEntity.Entity.GameObject.TransformedFrom == null)
        //        .Select(e => new SelectListItem
        //        {
        //            Value = e.EnemyId.ToString(),
        //            Text = e.HostileEntity.Entity.GameObject.GameObjectName
        //        })
        //        .ToList();

        //    var bossDrops = boss.BossDrops.ToList();
        //    for (int i = 0; i < 15; i++)
        //    {
        //        var drop = new BossDropEditViewModel();
        //        if (i < bossDrops.Count)
        //        {
        //            drop.ItemId = bossDrops[i].ItemId.ToString();
        //            drop.Quantity = bossDrops[i].Quantity;
        //        }
        //        viewModel.BossDrops.Add(drop);
        //    }

        //    var existingPartsData = new List<(
        //        BossPart part,
        //        List<(GameObject go, Entity entity, HostileEntity hostile, List<BossPartEnemies> enemies, List<EntityDrop> drops)>
        //    )>();

        //    foreach (var part in boss.BossParts)
        //    {
        //        var stagesData = new List<(GameObject go, Entity entity, HostileEntity hostile, List<BossPartEnemies> enemies, List<EntityDrop> drops)>();
        //        var current = part.HostileEntity.Entity.GameObject;

        //        while (current != null)
        //        {
        //            var entity = await _context.Entity
        //                .Include(e => e.EntityDrops)
        //                .FirstOrDefaultAsync(e => e.GameObjectName == current.GameObjectName);

        //            var enemies = await _context.BossPartEnemies
        //                .Where(bpe => bpe.BossPartId == part.BossPartId)
        //                .ToListAsync();

        //            if (entity != null)
        //            {
        //                stagesData.Add((current, entity, part.HostileEntity, enemies, entity.EntityDrops.ToList()));
        //            }

        //            current = await _context.GameObject
        //                .FirstOrDefaultAsync(go => go.GameObjectName == current.TransformName);
        //        }

        //        existingPartsData.Add((part, stagesData));
        //    }

        //    var bossParts = boss.BossParts.ToList();
        //    for (int i = 0; i < 5; i++)
        //    {
        //        var part = new BossPartEditViewModel();

        //        if (i < bossParts.Count)
        //        {
        //            var currentPart = bossParts[i];
        //            part.PartName = currentPart.HostileEntity.Entity.GameObject.GameObjectName;
        //            part.Description = currentPart.HostileEntity.Entity.GameObject.Description ?? string.Empty;
        //            part.Quantity = currentPart.Quantity;

        //            var stages = currentPart.HostileEntity.Entity.GameObject.GetAllStages().ToList();
        //            for (int j = 0; j < 2; j++)
        //            {
        //                var stage = new BossStageEditViewModel();

        //                if (j < stages.Count)
        //                {
        //                    var currentStage = stages[j];
        //                    var entity = await _context.Entity
        //                        .FirstOrDefaultAsync(e => e.GameObjectName == currentStage.GameObjectName);
        //                    var enemies = await _context.BossPartEnemies
        //                        .Include(bpe => bpe.Enemy)
        //                            .ThenInclude(e => e.HostileEntity)
        //                                .ThenInclude(he => he.Entity)
        //                                    .ThenInclude(e => e.GameObject)
        //                        .Where(bpe => bpe.BossPartId == currentPart.BossPartId)
        //                        .ToListAsync();

        //                    stage.Sprite = currentStage.Sprite;
        //                    stage.Hp = entity?.Hp ?? 0;
        //                    stage.Defense = entity?.Defense ?? 0;
        //                    stage.EntityId = entity?.EntityId ?? 0;
        //                    stage.ContactDamage = currentPart.HostileEntity.ContactDamage;

        //                    var spawnedEnemies = enemies.ToList();
        //                    for (int k = 0; k < 3; k++)
        //                    {
        //                        var enemy = new BossStageEnemyEditViewModel();
        //                        if (k < spawnedEnemies.Count)
        //                        {
        //                            enemy.EnemyId = spawnedEnemies[k].EnemyId.ToString();
        //                            enemy.Quantity = spawnedEnemies[k].Quantity;
        //                        }
        //                        stage.SpawnedEnemies.Add(enemy);
        //                    }

        //                    var drops = entity?.EntityDrops.ToList() ?? new List<EntityDrop>();
        //                    for (int k = 0; k < 3; k++)
        //                    {
        //                        var drop = new BossStageDropEditViewModel();
        //                        if (k < drops.Count)
        //                        {
        //                            drop.ItemId = drops[k].ItemId.ToString();
        //                            drop.Quantity = drops[k].Quantity;
        //                        }
        //                        stage.Drops.Add(drop);
        //                    }
        //                }
        //                else
        //                {
        //                    for (int k = 0; k < 3; k++)
        //                    {
        //                        stage.SpawnedEnemies.Add(new BossStageEnemyEditViewModel());
        //                        stage.Drops.Add(new BossStageDropEditViewModel());
        //                    }
        //                }

        //                part.Stages.Add(stage);
        //            }
        //        }
        //        else
        //        {
        //            for (int j = 0; j < 2; j++)
        //            {
        //                var stage = new BossStageEditViewModel();
        //                for (int k = 0; k < 3; k++)
        //                {
        //                    stage.SpawnedEnemies.Add(new BossStageEnemyEditViewModel());
        //                    stage.Drops.Add(new BossStageDropEditViewModel());
        //                }
        //                part.Stages.Add(stage);
        //            }
        //        }

        //        viewModel.BossParts.Add(part);
        //    }

        //    return View(viewModel);
        //}

        //// POST: Bosses/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Edit(BossEditViewModel viewModel)
        //{
        //    viewModel.AvailableItems = _context.Item
        //        .Include(i => i.GameObject)
        //        .Where(i => i.GameObject.TransformedFrom == null)
        //        .Select(i => new SelectListItem
        //        {
        //            Value = i.ItemId.ToString(),
        //            Text = i.GameObject.GameObjectName
        //        })
        //        .ToList();

        //    viewModel.AvailableEnemies = _context.Enemy
        //        .Include(e => e.HostileEntity)
        //            .ThenInclude(he => he.Entity)
        //                .ThenInclude(e => e.GameObject)
        //        .Where(e => e.HostileEntity.Entity.GameObject.TransformedFrom == null)
        //        .Select(e => new SelectListItem
        //        {
        //            Value = e.EnemyId.ToString(),
        //            Text = e.HostileEntity.Entity.GameObject.GameObjectName
        //        })
        //        .ToList();

        //    if (ModelState.IsValid)
        //    {
        //        var originalBoss = await _context.Boss
        //            .Include(b => b.BossDrops)
        //            .Include(b => b.BossParts)
        //                .ThenInclude(bp => bp.HostileEntity)
        //                    .ThenInclude(he => he.Entity)
        //                        .ThenInclude(e => e.GameObject)
        //            .Include(b => b.BossParts)
        //                .ThenInclude(bp => bp.HostileEntity)
        //                    .ThenInclude(he => he.Entity)
        //                        .ThenInclude(e => e.EntityDrops)
        //            .Include(b => b.BossParts)
        //                .ThenInclude(bp => bp.BossPartEnemies)
        //            .FirstOrDefaultAsync(b => b.BossName == viewModel.OriginalBossName);

        //        if (originalBoss == null)
        //        {
        //            return NotFound();
        //        }

        //        if (string.IsNullOrEmpty(viewModel.BossName))
        //        {
        //            ModelState.AddModelError("BossName", "Boss name is required");
        //            return View(viewModel);
        //        }

        //        if (viewModel.BossName.Length > 50)
        //        {
        //            ModelState.AddModelError("BossName", "Boss name cannot exceed 50 characters");
        //            return View(viewModel);
        //        }

        //        if (viewModel.OriginalBossName != viewModel.BossName &&
        //            await _context.Boss.AnyAsync(b => b.BossName == viewModel.BossName))
        //        {
        //            ModelState.AddModelError("BossName", "A boss with this name already exists");
        //            return View(viewModel);
        //        }

        //        var filledParts = viewModel.BossParts
        //            .Where(p => !string.IsNullOrEmpty(p.PartName))
        //            .ToList();

        //        if (!filledParts.Any())
        //        {
        //            ModelState.AddModelError("", "At least one boss part must be filled");
        //            return View(viewModel);
        //        }

        //        var partNames = filledParts.Select(p => p.PartName).ToList();
        //        if (partNames.Count != partNames.Distinct().Count())
        //        {
        //            ModelState.AddModelError("", "Part names must be unique");
        //            return View(viewModel);
        //        }

        //        var allGameObjectNames = new List<string>();
        //        var allSprites = new List<string>();
        //        var allEntityIds = new List<short>();

        //        foreach (var part in filledParts)
        //        {
        //            var filledStages = part.Stages
        //                .Where(s => !string.IsNullOrEmpty(s.Sprite))
        //                .ToList();

        //            if (!filledStages.Any())
        //            {
        //                ModelState.AddModelError("", $"Part '{part.PartName}' must have at least one stage");
        //                return View(viewModel);
        //            }

        //            foreach (var stage in filledStages)
        //            {
        //                if (stage.Hp < 0 || stage.Hp > 150000)
        //                {
        //                    ModelState.AddModelError("", "HP must be between 0 and 150000");
        //                    return View(viewModel);
        //                }
        //                if (stage.Defense < 0 || stage.Defense > 100)
        //                {
        //                    ModelState.AddModelError("", "Defense must be between 0 and 100");
        //                    return View(viewModel);
        //                }
        //                if (stage.ContactDamage < 0 || stage.ContactDamage > 500)
        //                {
        //                    ModelState.AddModelError("", "Contact damage must be between 0 and 500");
        //                    return View(viewModel);
        //                }
        //                if (stage.EntityId < -500 || stage.EntityId > 1000)
        //                {
        //                    ModelState.AddModelError("", "Entity ID must be between -500 and 1000");
        //                    return View(viewModel);
        //                }
        //            }

        //            for (int i = 0; i < filledStages.Count; i++)
        //            {
        //                var stage = filledStages[i];
        //                var gameObjectName = i == 0 ? part.PartName : $"{part.PartName}_{i + 1}";

        //                allGameObjectNames.Add(gameObjectName);
        //                allSprites.Add(stage.Sprite);
        //                allEntityIds.Add(stage.EntityId);
        //            }
        //        }

        //        if (allGameObjectNames.Count != allGameObjectNames.Distinct().Count())
        //        {
        //            ModelState.AddModelError("", "Game object names must be unique across all parts and stages");
        //            return View(viewModel);
        //        }

        //        if (allSprites.Count != allSprites.Distinct().Count())
        //        {
        //            ModelState.AddModelError("", "Sprites must be unique across all parts and stages");
        //            return View(viewModel);
        //        }

        //        if (allEntityIds.Count != allEntityIds.Distinct().Count())
        //        {
        //            ModelState.AddModelError("", "Entity IDs must be unique across all parts and stages");
        //            return View(viewModel);
        //        }

        //        var existingGameObjects = new List<GameObject>();
        //        var existingEntities = new List<Entity>();
        //        var existingHostileEntities = new List<HostileEntity>();
        //        var existingBossParts = new List<BossPart>();
        //        var existingBossPartEnemies = new List<BossPartEnemies>();
        //        var existingEntityDrops = new List<EntityDrop>();

        //        foreach (var part in originalBoss.BossParts)
        //        {
        //            existingBossParts.Add(part);
        //            existingHostileEntities.Add(part.HostileEntity);
        //            existingBossPartEnemies.AddRange(part.BossPartEnemies);

        //            var current = part.HostileEntity.Entity.GameObject;
        //            while (current != null && !existingGameObjects.Contains(current))
        //            {
        //                existingGameObjects.Add(current);
        //                var entity = await _context.Entity
        //                    .Include(e => e.EntityDrops)
        //                    .FirstOrDefaultAsync(e => e.GameObjectName == current.GameObjectName);
        //                if (entity != null)
        //                {
        //                    existingEntities.Add(entity);
        //                    existingEntityDrops.AddRange(entity.EntityDrops);
        //                }
        //                current = await _context.GameObject
        //                    .FirstOrDefaultAsync(go => go.GameObjectName == current.TransformName);
        //            }
        //        }

        //        var existingEntityIds = existingEntities.Select(e => e.EntityId).ToList();
        //        foreach (var entityId in allEntityIds)
        //        {
        //            if (!existingEntityIds.Contains(entityId) &&
        //                await _context.Entity.AnyAsync(e => e.EntityId == entityId))
        //            {
        //                ModelState.AddModelError("", $"Entity ID {entityId} is already in use");
        //                return View(viewModel);
        //            }
        //        }

        //        var existingNames = existingGameObjects.Select(go => go.GameObjectName).ToList();
        //        foreach (var name in allGameObjectNames)
        //        {
        //            if (!existingNames.Contains(name) &&
        //                await _context.GameObject.AnyAsync(go => go.GameObjectName == name))
        //            {
        //                ModelState.AddModelError("", $"Game object with name '{name}' already exists");
        //                return View(viewModel);
        //            }
        //        }

        //        var existingSprites = existingGameObjects.Select(go => go.Sprite).ToList();
        //        foreach (var sprite in allSprites)
        //        {
        //            if (!existingSprites.Contains(sprite) &&
        //                await _context.GameObject.AnyAsync(go => go.Sprite == sprite))
        //            {
        //                ModelState.AddModelError("", $"Sprite '{sprite}' already exists");
        //                return View(viewModel);
        //            }
        //        }

        //        _context.BossDrop.RemoveRange(originalBoss.BossDrops);

        //        originalBoss.BossName = viewModel.BossName;
        //        originalBoss.SummonItemId = !string.IsNullOrEmpty(viewModel.SummonItemId) ? short.Parse(viewModel.SummonItemId) : null;

        //        foreach (var drop in viewModel.BossDrops.Where(d => !string.IsNullOrEmpty(d.ItemId) && d.Quantity > 0))
        //        {
        //            var bossDrop = new BossDrop
        //            {
        //                BossName = originalBoss.BossName,
        //                ItemId = short.Parse(drop.ItemId),
        //                Quantity = (short)drop.Quantity
        //            };
        //            _context.BossDrop.Add(bossDrop);
        //        }

        //        var stageIndex = 0;

        //        foreach (var part in filledParts)
        //        {
        //            var filledStages = part.Stages
        //                .Where(s => !string.IsNullOrEmpty(s.Sprite))
        //                .ToList();

        //            GameObject? previousGameObject = null;

        //            for (int i = 0; i < filledStages.Count; i++)
        //            {
        //                var stage = filledStages[i];
        //                var gameObjectName = i == 0 ? part.PartName : $"{part.PartName}_{i + 1}";

        //                GameObject gameObject;
        //                Entity entity;
        //                HostileEntity hostileEntity;
        //                BossPart bossPart;

        //                if (stageIndex < existingGameObjects.Count)
        //                {
        //                    gameObject = existingGameObjects[stageIndex];
        //                    entity = existingEntities[stageIndex];
        //                    hostileEntity = existingHostileEntities[stageIndex / 2];
        //                    bossPart = existingBossParts[stageIndex / 2];

        //                    gameObject.GameObjectName = gameObjectName;
        //                    gameObject.Description = i == 0 ? part.Description : null;
        //                    gameObject.Sprite = stage.Sprite;
        //                    gameObject.TransformName = previousGameObject?.GameObjectName;

        //                    entity.EntityId = stage.EntityId;
        //                    entity.Hp = stage.Hp;
        //                    entity.Defense = (short)stage.Defense;

        //                    hostileEntity.ContactDamage = (short)stage.ContactDamage;

        //                    _context.GameObject.Update(gameObject);
        //                    _context.Entity.Update(entity);
        //                    _context.HostileEntity.Update(hostileEntity);

        //                    var oldEnemies = await _context.BossPartEnemies
        //                        .Where(bpe => bpe.BossPartId == bossPart.BossPartId)
        //                        .ToListAsync();
        //                    _context.BossPartEnemies.RemoveRange(oldEnemies);

        //                    var oldDrops = await _context.EntityDrop
        //                        .Where(ed => ed.EntityId == entity.EntityId)
        //                        .ToListAsync();
        //                    _context.EntityDrop.RemoveRange(oldDrops);
        //                }
        //                else
        //                {
        //                    gameObject = new GameObject
        //                    {
        //                        GameObjectName = gameObjectName,
        //                        Description = i == 0 ? part.Description : null,
        //                        Sprite = stage.Sprite,
        //                        TransformName = previousGameObject?.GameObjectName
        //                    };
        //                    _context.GameObject.Add(gameObject);

        //                    entity = new Entity
        //                    {
        //                        EntityId = stage.EntityId,
        //                        GameObjectName = gameObject.GameObjectName,
        //                        Hp = stage.Hp,
        //                        Defense = (short)stage.Defense
        //                    };
        //                    _context.Entity.Add(entity);

        //                    hostileEntity = new HostileEntity
        //                    {
        //                        EntityId = entity.EntityId,
        //                        ContactDamage = (short)stage.ContactDamage
        //                    };
        //                    _context.HostileEntity.Add(hostileEntity);

        //                    bossPart = new BossPart
        //                    {
        //                        BossName = originalBoss.BossName,
        //                        HostileEntityId = hostileEntity.HostileEntityId,
        //                        Quantity = (short)part.Quantity
        //                    };
        //                    _context.BossPart.Add(bossPart);
        //                }

        //                if (i == 0)
        //                {
        //                    foreach (var enemy in stage.SpawnedEnemies.Where(e => !string.IsNullOrEmpty(e.EnemyId) && e.Quantity > 0))
        //                    {
        //                        var bossPartEnemy = new BossPartEnemies
        //                        {
        //                            BossPartId = bossPart.BossPartId,
        //                            EnemyId = short.Parse(enemy.EnemyId),
        //                            Quantity = (short)enemy.Quantity
        //                        };
        //                        _context.BossPartEnemies.Add(bossPartEnemy);
        //                    }
        //                }

        //                foreach (var drop in stage.Drops.Where(d => !string.IsNullOrEmpty(d.ItemId) && d.Quantity > 0))
        //                {
        //                    var entityDrop = new EntityDrop
        //                    {
        //                        EntityId = entity.EntityId,
        //                        ItemId = short.Parse(drop.ItemId),
        //                        Quantity = (short)drop.Quantity
        //                    };
        //                    _context.EntityDrop.Add(entityDrop);
        //                }

        //                previousGameObject = gameObject;
        //                stageIndex++;
        //            }
        //        }

        //        for (int i = stageIndex; i < existingGameObjects.Count; i++)
        //        {
        //            _context.GameObject.Remove(existingGameObjects[i]);
        //        }

        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(viewModel);
        //}

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
                        .ThenInclude(e => e!.EntityDrops)
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

    public static class GameObjectExtensions
    {
        public static IEnumerable<GameObject> GetAllStages(this GameObject gameObject)
        {
            var stages = new List<GameObject>();
            var current = gameObject;
            while (current != null)
            {
                stages.Add(current);
                current = current.Transform;
            }
            return stages;
        }
    }
}
