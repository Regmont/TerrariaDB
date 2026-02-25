using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
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

            var enemies = await _context.Enemy
                .Select(e => new EntityItemViewModel
                {
                    Id = e.HostileEntity.Entity.EntityId.ToString(),
                    Name = e.HostileEntity.Entity.GameObject.GameObjectName,
                    Sprite = e.HostileEntity.Entity.GameObject.Sprite,
                    Type = "Enemy",
                    TypeId = e.EnemyId.ToString()
                })
                .ToListAsync();

            var townNpcs = await _context.TownNpc
                .Select(t => new EntityItemViewModel
                {
                    Id = t.Entity.EntityId.ToString(),
                    Name = t.Entity.GameObject.GameObjectName,
                    Sprite = t.Entity.GameObject.Sprite,
                    Type = "TownNpc",
                    TypeId = t.TownNpcId.ToString()
                })
                .ToListAsync();

            entities.AddRange(bosses);
            entities.AddRange(enemies);
            entities.AddRange(townNpcs);

            var viewModel = new EntityIndexViewModel
            {
                Entities = entities
            };

            return View(viewModel);
        }
    }
}
