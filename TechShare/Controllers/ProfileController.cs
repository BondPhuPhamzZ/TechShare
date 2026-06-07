using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TechShare.Data;
using TechShare.Enums;
using TechShare.ViewModels;

namespace TechShare.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được vào trang này
    public class ProfileController : Controller
    {
        private readonly TechShareDbContext _context;

        public ProfileController(TechShareDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy ID người dùng đang đăng nhập từ Cookie
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var viewModel = new ProfileViewModel();

            // 1. Lấy thông tin cá nhân
            viewModel.UserInfo = await _context.Users.FindAsync(userId);

            // 2. Lấy thiết bị đã đăng (Cửa hàng của tôi)
            viewModel.MyPostedDevices = await _context.Devices
                .Include(d => d.Category)
                .Where(d => d.OwnerId == userId)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            // 3. Lấy thiết bị ĐANG THUÊ (Để dễ quản lý tài sản đang cầm)
            viewModel.MyActiveRentals = await _context.Rentals
                .Include(r => r.Device)
                .ThenInclude(d => d.Owner)
                .Where(r => r.RenterId == userId && (r.Status == RentalStatus.Active || r.Status == RentalStatus.Approved_PendingHandover || r.Status == RentalStatus.Disputed))
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(viewModel);
        }

        // Bật / Tắt trạng thái cho thuê của máy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDeviceStatus(int deviceId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId && d.OwnerId == userId);
            
            if (device != null)
            {
                if (device.Status == DeviceStatus.Available)
                {
                    device.Status = DeviceStatus.OutOfStock;
                    device.StockQuantity = 0; // Ẩn khỏi trang chủ
                }
                else
                {
                    device.Status = DeviceStatus.Available;
                    device.StockQuantity = 1; // Hiện lại
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
