using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TechShare.Data;
using TechShare.Enums;
using TechShare.ViewModels;

namespace TechShare.Controllers
{
    [Authorize]
    public class HostController : Controller
    {
        private readonly TechShareDbContext _context;

        public HostController(TechShareDbContext context)
        {
            _context = context;
        }

        // Chủ máy -> Quản lý thiết bị đã đăng & Đơn khách đặt thuê
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) 
                return RedirectToAction("Login", "Auth");

            var viewModel = new HostViewModel();

            // Kho máy của tôi
            viewModel.MyPostedDevices = await _context.Devices
                .Include(d => d.Category)
                .Where(d => d.OwnerId == userId)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            // Đơn khách đang thuê máy của tôi
            viewModel.MyOrders = await _context.Rentals
                .Include(r => r.Device)
                .Include(r => r.Renter)
                .Where(r => r.Device.OwnerId == userId && (r.Status == RentalStatus.ChoDuyet || r.Status == RentalStatus.ChoGiao || r.Status == RentalStatus.TranhChap || r.Status == RentalStatus.ChoTra || r.Status == RentalStatus.DangCheck))
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
                if (device.Status == DeviceStatus.SanSang)
                {
                    device.Status = DeviceStatus.HetHang;
                    device.StockQuantity = 0; // Ẩn khỏi trang chủ
                }
                else
                {
                    device.Status = DeviceStatus.SanSang;
                    device.StockQuantity = 1; // Hiện lại
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Chủ máy duyệt đơn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.ChoDuyet)
            {
                rental.Status = RentalStatus.ChoGiao;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Báo đã giao máy (Chờ khách check)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsHandedOver(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.ChoGiao)
            {
                rental.Status = RentalStatus.DangCheck;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Chủ máy xác nhận đã nhận lại máy và hoàn cọc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReturn(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.ChoTra)
            {
                rental.Status = RentalStatus.HoanTat;
                rental.DepositStatus = DepositStatus.DaHoanTien; // Hoàn cọc
                
                // Tăng lại số lượng máy
                var device = await _context.Devices.FindAsync(rental.DeviceId);
                if (device != null)
                {
                    device.StockQuantity += rental.Quantity;
                    if(device.StockQuantity > 0)
                    {
                        device.Status = DeviceStatus.SanSang;
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Xử lý Tranh Chấp: Chủ máy chấp nhận lỗi -> Hủy đơn & Hoàn Cọc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveDisputeRefund(int id)
        {
            var rental = await _context.Rentals.Include(r => r.Device).FirstOrDefaultAsync(r => r.Id == id);
            if (rental != null && rental.Status == RentalStatus.TranhChap)
            {
                rental.Status = RentalStatus.DaHuy; 
                rental.DepositStatus = DepositStatus.DaHoanTien; // Hoàn cọc (Tranh chấp do lỗi hệ thống/chủ)
                
                // Trả máy lại kho
                rental.Device.StockQuantity += rental.Quantity;
                if (rental.Device.StockQuantity > 0) 
                    rental.Device.Status = DeviceStatus.SanSang;
                
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Xử lý Tranh Chấp: Chủ máy xác định lỗi do khách làm hỏng -> Giữ cọc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveDisputeRetain(int id)
        {
            var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.Id == id);
            if (rental != null && rental.Status == RentalStatus.TranhChap)
            {
                rental.Status = RentalStatus.HoanTat; 
                rental.DepositStatus = DepositStatus.GiuCoc; // Giữ cọc (Tranh chấp do lỗi khách)
                
                // Không cộng lại máy (máy đã hỏng/mất)
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
