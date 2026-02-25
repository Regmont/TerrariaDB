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
        public IActionResult Create()
        {
            ViewData["EntityId"] = new SelectList(_context.Entity, "EntityId", "GameObjectName");
            return View();
        }

        // POST: TownNpcs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TownNpcId,EntityId")] TownNpc townNpc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(townNpc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EntityId"] = new SelectList(_context.Entity, "EntityId", "GameObjectName", townNpc.EntityId);
            return View(townNpc);
        }

        // GET: TownNpcs/Edit/5
        public async Task<IActionResult> Edit(byte? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var townNpc = await _context.TownNpc.FindAsync(id);
            if (townNpc == null)
            {
                return NotFound();
            }
            ViewData["EntityId"] = new SelectList(_context.Entity, "EntityId", "GameObjectName", townNpc.EntityId);
            return View(townNpc);
        }

        // POST: TownNpcs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(byte id, [Bind("TownNpcId,EntityId")] TownNpc townNpc)
        {
            if (id != townNpc.TownNpcId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(townNpc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TownNpcExists(townNpc.TownNpcId))
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
            ViewData["EntityId"] = new SelectList(_context.Entity, "EntityId", "GameObjectName", townNpc.EntityId);
            return View(townNpc);
        }

        // GET: TownNpcs/Delete/5
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

        private bool TownNpcExists(byte id)
        {
            return _context.TownNpc.Any(e => e.TownNpcId == id);
        }
    }
}
