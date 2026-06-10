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

        public async Task<IActionResult> Index(string? searchQuery, int? categoryId)
        {
            var query = _context.Devices
                .Include(d => d.Owner)    
                .Include(d => d.Category) 
                .Where(d => d.Status == DeviceStatus.Available && d.StockQuantity > 0);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(d => d.Name.Contains(searchQuery));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            var devices = await query
                .OrderByDescending(d => d.Id) 
                .ToListAsync();

            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId);
            ViewBag.SearchQuery = searchQuery;

            return View(devices);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
