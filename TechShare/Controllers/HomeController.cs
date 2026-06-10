using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechShare.Models;
using Microsoft.EntityFrameworkCore;
using TechShare.Data;
using TechShare.Enums;
namespace TechShare.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TechShareDbContext _context; 

        public HomeController(ILogger<HomeController> logger, TechShareDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string q, int? categoryId)
        {
            var query = _context.Devices
                .Include(d => d.Owner)    
                .Include(d => d.Category) 
                .Where(d => d.Status == DeviceStatus.Available && d.StockQuantity > 0);

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(d => d.Name.Contains(q));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            var devices = await query
                .OrderByDescending(d => d.Id) 
                .ToListAsync();

            ViewData["SearchQuery"] = q;
            ViewData["CategoryId"] = categoryId;
            ViewData["Categories"] = await _context.Categories.ToListAsync();

            return View(devices);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
