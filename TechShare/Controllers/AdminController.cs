using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShare.Data;
using System.Threading.Tasks;
using System.Linq;

namespace TechShare.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly TechShareDbContext _context;

        public AdminController(TechShareDbContext context)
        {
            _context = context;
        }

        // Bảng điều khiển (Thống kê)
        public async Task<IActionResult> Index()
        {
            // Lấy số liệu thống kê
            var totalUsers = await _context.Users.Where(u => u.Role == "User").CountAsync();
            var totalDevices = await _context.Devices.CountAsync();
            var totalOrders = await _context.Rentals.CountAsync();
            
            // Tính tổng doanh thu từ các đơn hàng đã Hoàn Tất
            var totalRevenue = await _context.Rentals
                .Where(r => r.Status == Enums.RentalStatus.HoanTat)
                .SumAsync(r => r.TotalPrice); // Mặc định là Cửa hàng lấy 100%

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalDevices = totalDevices;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;

            // Đơn hàng cần xử lý (Chờ duyệt)
            var pendingOrders = await _context.Rentals
                .Include(r => r.Renter)
                .Include(r => r.Device)
                .Where(r => r.Status == Enums.RentalStatus.ChoDuyet)
                .OrderByDescending(r => r.Id)
                .Take(5)
                .ToListAsync();

            return View(pendingOrders);
        }

        // Quản lý Thiết bị
        public async Task<IActionResult> Devices()
        {
            var devices = await _context.Devices.Include(d => d.Category).OrderByDescending(d => d.Id).ToListAsync();
            return View(devices);
        }

        // Quản lý Danh mục
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.Include(c => c.Devices).OrderByDescending(c => c.Id).ToListAsync();
            return View(categories);
        }

        // Quản lý Đơn thuê
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Rentals
                .Include(r => r.Renter)
                .Include(r => r.Device)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
            return View(orders);
        }

        // Quản lý Khách hàng
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.Where(u => u.Role == "User").OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        // Quản lý Đánh giá
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Rental)
                    .ThenInclude(ren => ren.Device)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reviews);
        }
    }
}
