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
    public class CurrencyTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CurrencyTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CurrencyTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.CurrencyType.ToListAsync());
        }
    }
}
