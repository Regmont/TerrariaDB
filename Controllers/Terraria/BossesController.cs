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
        public async Task<IActionResult> Create([Bind("BossName,SummonItemId")] Boss boss)
        {
            if (ModelState.IsValid)
            {
                _context.Add(boss);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SummonItemId"] = new SelectList(_context.Item, "ItemId", "CurrencyName", boss.SummonItemId);
            return View(boss);
        }

        // GET: Bosses/Edit/5
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
        public async Task<IActionResult> Edit(string id, [Bind("BossName,SummonItemId")] Boss boss)
        {
            if (id != boss.BossName)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(boss);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BossExists(boss.BossName))
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
            ViewData["SummonItemId"] = new SelectList(_context.Item, "ItemId", "CurrencyName", boss.SummonItemId);
            return View(boss);
        }

        // GET: Bosses/Delete/5
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

        private bool BossExists(string id)
        {
            return _context.Boss.Any(e => e.BossName == id);
        }
    }
}
