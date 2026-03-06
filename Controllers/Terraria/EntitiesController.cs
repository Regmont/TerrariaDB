using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;
using TerrariaDB.ViewModels.Terraria.Entity;

namespace TerrariaDB.Controllers.Terraria
{
    public class EntitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EntitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Entities
        public async Task<IActionResult> Index()
        {
            var entities = new List<EntityItemViewModel>();

            var bosses = await _context.BossPart
                .Select(bp => new EntityItemViewModel
                {
                    Id = bp.HostileEntity.Entity.EntityId.ToString(),
                    Name = bp.HostileEntity.Entity.GameObject.GameObjectName,
                    Sprite = bp.HostileEntity.Entity.GameObject.Sprite,
                    Type = "BossPart",
                    TypeId = bp.BossName
                })
                .ToListAsync();
            entities.AddRange(bosses);

            var enemies = await _context.Enemy
                .Include(e => e.HostileEntity)
                    .ThenInclude(he => he.Entity)
                        .ThenInclude(en => en.GameObject)
                            .ThenInclude(g => g.TransformedFrom)
                .ToListAsync();

            foreach (var enemy in enemies)
            {
                var rootEnemy = await FindRootEnemy(enemy);
                entities.Add(new EntityItemViewModel
                {
                    Id = enemy.HostileEntity.Entity.EntityId.ToString(),
                    Name = enemy.HostileEntity.Entity.GameObject.GameObjectName,
                    Sprite = enemy.HostileEntity.Entity.GameObject.Sprite,
                    Type = "Enemy",
                    TypeId = rootEnemy.EnemyId.ToString()
                });
            }

            var townNpcs = await _context.TownNpc
                .Include(t => t.Entity)
                    .ThenInclude(e => e.GameObject)
                        .ThenInclude(g => g.TransformedFrom)
                .ToListAsync();

            foreach (var townNpc in townNpcs)
            {
                var rootTownNpc = await FindRootTownNpc(townNpc);
                entities.Add(new EntityItemViewModel
                {
                    Id = townNpc.Entity.EntityId.ToString(),
                    Name = townNpc.Entity.GameObject.GameObjectName,
                    Sprite = townNpc.Entity.GameObject.Sprite,
                    Type = "TownNpc",
                    TypeId = rootTownNpc.TownNpcId.ToString()
                });
            }

            var viewModel = new EntityIndexViewModel
            {
                Entities = entities
            };

            return View(viewModel);
        }

        private async Task<Enemy> FindRootEnemy(Enemy enemy)
        {
            var currentEnemy = enemy;

            while (true)
            {
                var gameObject = currentEnemy.HostileEntity.Entity.GameObject;
                if (gameObject.TransformedFrom == null)
                {
                    return currentEnemy;
                }

                var previousGameObjectName = gameObject.TransformedFrom.GameObjectName;
                var previousEnemy = await _context.Enemy
                    .Include(e => e.HostileEntity)
                        .ThenInclude(he => he.Entity)
                            .ThenInclude(en => en.GameObject)
                    .FirstOrDefaultAsync(e => e.HostileEntity.Entity.GameObjectName == previousGameObjectName);

                if (previousEnemy == null)
                {
                    return currentEnemy;
                }

                currentEnemy = previousEnemy;
            }
        }

        private async Task<TownNpc> FindRootTownNpc(TownNpc townNpc)
        {
            var currentTownNpc = townNpc;

            while (true)
            {
                var gameObject = currentTownNpc.Entity.GameObject;
                if (gameObject.TransformedFrom == null)
                {
                    return currentTownNpc;
                }

                var previousGameObjectName = gameObject.TransformedFrom.GameObjectName;
                var previousTownNpc = await _context.TownNpc
                    .Include(t => t.Entity)
                        .ThenInclude(e => e.GameObject)
                    .FirstOrDefaultAsync(t => t.Entity.GameObjectName == previousGameObjectName);

                if (previousTownNpc == null)
                {
                    return currentTownNpc;
                }

                currentTownNpc = previousTownNpc;
            }
        }
    }
}
