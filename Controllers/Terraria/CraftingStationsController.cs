using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Data;
using TerrariaDB.Models.Terraria;

namespace TerrariaDB.Controllers.Terraria
{
    public class CraftingStationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CraftingStationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CraftingStations
        public async Task<IActionResult> Index()
        {
            return View(await _context.CraftingStation.ToListAsync());
        }
    }
}
