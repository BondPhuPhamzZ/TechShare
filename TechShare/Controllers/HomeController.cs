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

        public async Task<IActionResult> Index()
        {
            var devices = await _context.Devices
                .Include(d => d.Owner)    
                .Include(d => d.Category) 
                .Where(d => d.Status == DeviceStatus.Available && d.StockQuantity > 0)
                .OrderByDescending(d => d.Id) 
                .ToListAsync();

            return View(devices);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
