using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;
using TerrariaDB.ViewModels.Terraria.Enemy;

namespace TerrariaDB.Controllers.Terraria
{
    public class EnemiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnemiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Enemies
        public async Task<IActionResult> Index()
        {
            var enemies = await _context.Enemy
                .Where(e => e.HostileEntity.Entity.GameObject.TransformedFrom == null)
                .Select(e => new EnemyItemViewModel
                {
                    Id = e.EnemyId.ToString(),
                    Name = e.HostileEntity.Entity.GameObject.GameObjectName,
                    Sprite = e.HostileEntity.Entity.GameObject.Sprite
                })
                .ToListAsync();

            var viewModel = new EnemyIndexViewModel
            {
                Enemies = enemies
            };

            return View(viewModel);
        }

        // GET: Enemies/Details/5
        public async Task<IActionResult> Details(short id)
        {
            var enemy = await _context.Enemy
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                        .ThenInclude(en => en.GameObject)
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                        .ThenInclude(en => en.EntityDrops)
                            .ThenInclude(ed => ed.Item)
                                .ThenInclude(i => i.GameObject)
                .FirstOrDefaultAsync(e => e.EnemyId == id);

            if (enemy == null)
            {
                return NotFound();
            }

            var viewModel = new EnemyDetailsViewModel
            {
                EnemyId = enemy.EnemyId.ToString(),
                Name = enemy.HostileEntity.Entity.GameObject.GameObjectName,
                Description = enemy.HostileEntity.Entity.GameObject.Description ?? string.Empty,
                Sprite = enemy.HostileEntity.Entity.GameObject.Sprite,
                EntityId = enemy.HostileEntity.EntityId.ToString(),
                Hp = enemy.HostileEntity.Entity.Hp ?? 0,
                Defense = enemy.HostileEntity.Entity.Defense,
                ContactDamage = enemy.HostileEntity.ContactDamage,
                Drops = enemy.HostileEntity.Entity.EntityDrops.Select(ed => new EnemyDropViewModel
                {
                    Name = ed.Item.GameObject.GameObjectName,
                    Sprite = ed.Item.GameObject.Sprite,
                    Quantity = ed.Quantity
                }).ToList(),
                Transformations = GetTransformations(enemy.HostileEntity.Entity.GameObject, enemy)
            };

            return View(viewModel);
        }

        private List<EnemyTransformationViewModel> GetTransformations(GameObject gameObject, Enemy enemy)
        {
            var transformations = new List<EnemyTransformationViewModel>();

            var current = gameObject.Transform;
            while (current != null)
            {
                transformations.Add(new EnemyTransformationViewModel
                {
                    Name = current.GameObjectName,
                    Sprite = current.Sprite,
                    EntityId = enemy.HostileEntity.EntityId.ToString(),
                    Hp = enemy.HostileEntity.Entity.Hp ?? 0,
                    Defense = enemy.HostileEntity.Entity.Defense,
                    ContactDamage = enemy.HostileEntity.ContactDamage,
                    Drops = enemy.HostileEntity.Entity.EntityDrops.Select(ed => new EnemyDropViewModel
                    {
                        Name = ed.Item.GameObject.GameObjectName,
                        Sprite = ed.Item.GameObject.Sprite,
                        Quantity = ed.Quantity
                    }).ToList()
                });

                current = current.Transform;
            }

            return transformations;
        }

        // GET: Enemies/Create
        public IActionResult Create()
        {
            var viewModel = new EnemyCreateViewModel();

            viewModel.AvailableItems = _context.Item
                .Include(i => i.GameObject)
                .Where(i => i.GameObject.TransformedFrom == null)
                .Select(i => new SelectListItem
                {
                    Value = i.ItemId.ToString(),
                    Text = i.GameObject.GameObjectName
                })
                .ToList();

            for (int i = 0; i < 3; i++)
            {
                var stage = new EnemyStageViewModel();

                for (int j = 0; j < 5; j++)
                {
                    stage.Drops.Add(new EnemyDropCreateViewModel());
                }

                viewModel.Stages.Add(stage);
            }

            return View(viewModel);
        }

        // POST: Enemies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnemyId,HostileEntityId")] Enemy enemy)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enemy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["HostileEntityId"] = new SelectList(_context.HostileEntity, "HostileEntityId", "HostileEntityId", enemy.HostileEntityId);
            return View(enemy);
        }

        // GET: Enemies/Edit/5
        public IActionResult Edit(int id)
        {
            var enemy = _context.Enemy
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                    .ThenInclude(e => e.GameObject)
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                    .ThenInclude(e => e.EntityDrops)
                    .ThenInclude(ed => ed.Item)
                    .ThenInclude(i => i.GameObject)
                .FirstOrDefault(e => e.EnemyId == id);

            if (enemy == null)
            {
                return NotFound();
            }

            var viewModel = new EnemyEditViewModel
            {
                EnemyId = enemy.EnemyId.ToString(),
                Name = enemy.HostileEntity.Entity.GameObject.GameObjectName,
                Description = enemy.HostileEntity.Entity.GameObject.Description ?? string.Empty
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

            for (int i = 0; i < 3; i++)
            {
                var stage = new EnemyEditStageViewModel();

                if (i == 0)
                {
                    stage.Sprite = enemy.HostileEntity.Entity.GameObject.Sprite;
                    stage.Hp = enemy.HostileEntity.Entity.Hp ?? 0;
                    stage.Defense = enemy.HostileEntity.Entity.Defense;
                    stage.EntityId = enemy.HostileEntity.Entity.EntityId;
                    stage.ContactDamage = enemy.HostileEntity.ContactDamage;

                    var drops = enemy.HostileEntity.Entity.EntityDrops.ToList();
                    for (int j = 0; j < 5; j++)
                    {
                        var drop = new EnemyDropEditViewModel();
                        if (j < drops.Count)
                        {
                            drop.ItemId = drops[j].ItemId.ToString();
                            drop.Quantity = drops[j].Quantity;
                        }
                        stage.Drops.Add(drop);
                    }
                }
                else
                {
                    for (int j = 0; j < 5; j++)
                    {
                        stage.Drops.Add(new EnemyDropEditViewModel());
                    }
                }

                viewModel.Stages.Add(stage);
            }

            return View(viewModel);
        }

        // POST: Enemies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, [Bind("EnemyId,HostileEntityId")] Enemy enemy)
        {
            if (id != enemy.EnemyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enemy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnemyExists(enemy.EnemyId))
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
            ViewData["HostileEntityId"] = new SelectList(_context.HostileEntity, "HostileEntityId", "HostileEntityId", enemy.HostileEntityId);
            return View(enemy);
        }

        // GET: Enemies/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            var enemy = await _context.Enemy
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                        .ThenInclude(en => en.GameObject)
                .FirstOrDefaultAsync(e => e.EnemyId == id);

            if (enemy == null)
            {
                return NotFound();
            }

            var viewModel = new EnemyDeleteViewModel
            {
                EnemyId = enemy.EnemyId.ToString(),
                Name = enemy.HostileEntity.Entity.GameObject.GameObjectName,
                Sprite = enemy.HostileEntity.Entity.GameObject.Sprite
            };

            return View(viewModel);
        }

        // POST: Enemies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var enemy = await _context.Enemy
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                        .ThenInclude(en => en.GameObject)
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                        .ThenInclude(en => en.EntityDrops)
                .Include(e => e.BossPartEnemies)
                .FirstOrDefaultAsync(e => e.EnemyId == id);

            if (enemy == null)
            {
                return NotFound();
            }

            var allBossPartEnemies = new List<BossPartEnemies>();
            var allEntityDrops = new List<EntityDrop>();
            var allGameObjects = new List<GameObject>();

            await CollectEnemyData(enemy, allBossPartEnemies, allEntityDrops, allGameObjects);

            _context.BossPartEnemies.RemoveRange(allBossPartEnemies);
            _context.EntityDrop.RemoveRange(allEntityDrops);

            foreach (var go in allGameObjects)
            {
                _context.GameObject.Remove(go);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task CollectEnemyData(Enemy enemy,
            List<BossPartEnemies> bossPartEnemies,
            List<EntityDrop> entityDrops,
            List<GameObject> gameObjects)
        {
            var current = enemy.HostileEntity.Entity.GameObject;

            while (current != null && !gameObjects.Contains(current))
            {
                gameObjects.Add(current);

                var enemyAtStage = await _context.Enemy
                    .Include(e => e.BossPartEnemies)
                    .Include(e => e.HostileEntity)
                        .ThenInclude(he => he.Entity)
                            .ThenInclude(en => en.EntityDrops)
                    .FirstOrDefaultAsync(e => e.HostileEntity.Entity.GameObjectName == current.GameObjectName);

                if (enemyAtStage != null)
                {
                    bossPartEnemies.AddRange(enemyAtStage.BossPartEnemies);
                    entityDrops.AddRange(enemyAtStage.HostileEntity.Entity.EntityDrops);
                }

                current = await _context.GameObject
                    .FirstOrDefaultAsync(go => go.GameObjectName == current.TransformName);
            }
        }

        private bool EnemyExists(short id)
        {
            return _context.Enemy.Any(e => e.EnemyId == id);
        }
    }
}
