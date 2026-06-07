using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechShare.Data;
using TechShare.Enums;
using TechShare.ViewModels;

using Microsoft.AspNetCore.Authorization;

namespace TechShare.Controllers
{
    [Authorize] // Bắt buộc phải đăng nhập
    public class DashboardController : Controller
    {
        private readonly TechShareDbContext _context;

        public DashboardController(TechShareDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy ID thật của người đang đăng nhập
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            var viewModel = new DashboardViewModel();

            // 1. Lấy Đơn mình ĐI THUÊ
            viewModel.MyRentals = await _context.Rentals
                .Include(r => r.Device)
                .ThenInclude(d => d.Owner)
                .Where(r => r.RenterId == userId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            // 2. Lấy Đơn người khác THUÊ MÁY CỦA MÌNH
            viewModel.MyOrders = await _context.Rentals
                .Include(r => r.Device)
                .Include(r => r.Renter)
                .Where(r => r.Device.OwnerId == userId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(viewModel);
        }


        // 1. Chủ máy Duyệt đơn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.Pending)
            {
                rental.Status = RentalStatus.Approved_PendingHandover;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // 2. Chủ máy xác nhận mang máy đi giao -> Chờ khách đồng kiểm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Handover(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.Approved_PendingHandover)
            {
                // BƯỚC CẢI TIẾN: Thay vì kích hoạt ngay, ta chuyển sang trạng thái chờ Khách xác nhận
                rental.Status = RentalStatus.PendingRenterConfirmation; 
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // 2.5 Khách xác nhận ĐÃ TEST MÁY & NHẬN -> Kích hoạt 2H
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmHandover(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.PendingRenterConfirmation)
            {
                rental.Status = RentalStatus.Active;
                rental.ActualHandoverTime = DateTime.Now; // CHÍNH THỨC kích hoạt bộ đếm 2H
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // 3. Khách thuê Báo lỗi (Chỉ được phép trong 2H đầu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportIssue(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.Active && rental.ActualHandoverTime.HasValue)
            {
                var timePassed = DateTime.Now - rental.ActualHandoverTime.Value;
                if (timePassed.TotalHours <= 2)
                {
                    rental.Status = RentalStatus.Disputed;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index");
        }

        // 4. Khách thuê Trả máy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnDevice(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.Active)
            {
                rental.Status = RentalStatus.Returned_PendingInspection;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // 5. Chủ máy Xác nhận nhận lại máy & Hoàn cọc (Kết thúc vòng đời)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            var rental = await _context.Rentals.Include(r => r.Device).FirstOrDefaultAsync(r => r.Id == id);
            if (rental != null && rental.Status == RentalStatus.Returned_PendingInspection)
            {
                rental.Status = RentalStatus.Completed;
                rental.DepositStatus = DepositStatus.Refunded;
                
                // Trả lại số lượng tồn kho lên sàn
                rental.Device.StockQuantity += rental.Quantity;
                if (rental.Device.StockQuantity > 0) rental.Device.Status = DeviceStatus.Available;
                
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // 6. Xử lý Tranh Chấp: Chủ máy chấp nhận lỗi -> Hủy đơn & Hoàn Cọc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveDisputeRefund(int id)
        {
            var rental = await _context.Rentals.Include(r => r.Device).FirstOrDefaultAsync(r => r.Id == id);
            if (rental != null && rental.Status == RentalStatus.Disputed)
            {
                rental.Status = RentalStatus.Cancelled; // Đơn bị hủy do lỗi
                rental.DepositStatus = DepositStatus.Refunded; // Trả lại cọc cho sinh viên
                
                rental.Device.StockQuantity += rental.Quantity; // Cập nhật lại kho
                if (rental.Device.StockQuantity > 0) rental.Device.Status = DeviceStatus.Available;
                
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // 7. Xử lý Tranh Chấp: Chủ máy xác định lỗi do khách làm hỏng -> Giữ cọc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveDisputeRetain(int id)
        {
            var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.Id == id);
            if (rental != null && rental.Status == RentalStatus.Disputed)
            {
                rental.Status = RentalStatus.Completed; // Vẫn đóng đơn
                rental.DepositStatus = DepositStatus.Retained; // Tịch thu cọc của khách
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
