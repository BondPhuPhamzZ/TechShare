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

        public async Task<IActionResult> Index(int? month, int? year)
        {
            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;

            var totalUsers = await _context.Users.Where(u => u.Role == "User").CountAsync();
            var totalDevices = await _context.Devices.CountAsync();
            var totalOrders = await _context.Rentals.CountAsync();
            
            var totalRevenue = await _context.Rentals
                .Where(r => r.Status == Enums.RentalStatus.HoanTat && r.ActualReturnDate.HasValue)
                .Where(r => r.ActualReturnDate.Value.Month == selectedMonth && r.ActualReturnDate.Value.Year == selectedYear)
                .SumAsync(r => r.TotalPrice); 

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalDevices = totalDevices;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;

            var pendingOrders = await _context.Rentals
                .Include(r => r.Renter)
                .Include(r => r.Device)
                .Where(r => r.Status == Enums.RentalStatus.ChoDuyet)
                .OrderByDescending(r => r.Id)
                .Take(5)
                .ToListAsync();

            return View(pendingOrders);
        }

        public async Task<IActionResult> Devices()
        {
            var devices = await _context.Devices.Include(d => d.Category).OrderByDescending(d => d.Id).ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(devices);
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.Include(c => c.Devices).OrderByDescending(c => c.Id).ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Rentals
                .Include(r => r.Renter)
                .Include(r => r.Device)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.Where(u => u.Role == "User").OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

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

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa đánh giá vi phạm thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đánh giá.";
            }
            return RedirectToAction("Reviews");
        }
        [HttpPost]
        public async Task<IActionResult> ToggleUserLock(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "User");
            if (user != null)
            {
                user.IsLocked = !user.IsLocked;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = user.IsLocked ? $"Đã KHÓA tài khoản {user.FullName}." : $"Đã MỞ KHÓA tài khoản {user.FullName}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
            }
            return RedirectToAction("Users");
        }

        // Duyệt CCCD
        [HttpPost]
        public async Task<IActionResult> ApproveUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "User");
            if (user != null)
            {
                user.IsVerified = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã duyệt CCCD cho khách hàng {user.FullName}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, Enums.RentalStatus newStatus)
        {
            var rental = await _context.Rentals
                .Include(r => r.Device)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Orders");
            }

            // Hủy đơn/ Trả máy -> Trả lại số lượng máy vào kho
            if ((newStatus == Enums.RentalStatus.DaHuy && rental.Status != Enums.RentalStatus.DaHuy) ||
                (newStatus == Enums.RentalStatus.HoanTat && rental.Status != Enums.RentalStatus.HoanTat))
            {
                rental.Device.StockQuantity += rental.Quantity;
                
                // Nếu là Hoàn Tất (Trả máy thành công), tính toán trả lố ngày
                if (newStatus == Enums.RentalStatus.HoanTat)
                {
                    rental.ActualReturnDate = DateTime.Now;
                    
                    if (rental.ActualReturnDate.Value.Date > rental.EndDate.Date)
                    {
                        int lateDays = (rental.ActualReturnDate.Value.Date - rental.EndDate.Date).Days;
                        // Phạt 150% giá thuê một ngày
                        rental.LateFee = lateDays * rental.Device.PricePerDay * 1.5m;
                        rental.TotalPrice += rental.LateFee;
                    }
                }
            }

            // Ghi nhận thời điểm khách nhận máy (bắt đầu tính 2h)
            if (newStatus == Enums.RentalStatus.DangThue && 
               (rental.Status == Enums.RentalStatus.DangGiao || rental.Status == Enums.RentalStatus.DaDuyet))
            {
                rental.ShipTime = DateTime.Now;
            }

            rental.Status = newStatus;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn #{rental.Id} thành công!";
            return RedirectToAction("Orders");
        }

        // Quản lý Danh Mục - Thêm
        [HttpPost]
        public async Task<IActionResult> CreateCategory(string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var cat = new TechShare.Models.Category { Name = categoryName };
                _context.Categories.Add(cat);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm danh mục mới!";
            }
            return RedirectToAction("Categories");
        }

        // Quản lý Danh Mục - Sửa
        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, string categoryName)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat != null && !string.IsNullOrWhiteSpace(categoryName))
            {
                cat.Name = categoryName;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã cập nhật tên danh mục!";
            }
            return RedirectToAction("Categories");
        }

        // Quản lý Danh Mục - Xóa
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.Categories.Include(c => c.Devices).FirstOrDefaultAsync(c => c.Id == id);
            if (cat != null)
            {
                if (cat.Devices != null && cat.Devices.Any())
                {
                    TempData["ErrorMessage"] = "Không thể xóa danh mục đang có thiết bị!";
                }
                else
                {
                    _context.Categories.Remove(cat);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa danh mục!";
                }
            }
            return RedirectToAction("Categories");
        }
    }
}
