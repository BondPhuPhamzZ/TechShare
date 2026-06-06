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
        private readonly TechShareDbContext _context; // Inject DbContext

        public HomeController(ILogger<HomeController> logger, TechShareDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Cập nhật: Dùng chuẩn Async/Await để tăng hiệu suất xử lý đồng thời
        public async Task<IActionResult> Index()
        {
            // Flow: Truy vấn DB lấy danh sách thiết bị Sẵn sàng & Còn hàng (Bất đồng bộ)
            var devices = await _context.Devices
                .Include(d => d.Owner)    // Join bảng User để lấy điểm Uy tín
                .Include(d => d.Category) // Join bảng Category để lấy tên danh mục
                .Where(d => d.Status == DeviceStatus.Available && d.StockQuantity > 0)
                .OrderByDescending(d => d.Id) // Hiển thị máy mới nhất lên đầu
                .ToListAsync();

            // Truyền dữ liệu (List) sang View
            return View(devices);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
