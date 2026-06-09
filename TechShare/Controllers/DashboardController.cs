using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechShare.Data;
using TechShare.Enums;
using TechShare.ViewModels;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TechShare.Controllers
{
    [Authorize] 
    public class DashboardController : Controller
    {
        private readonly TechShareDbContext _context;

        public DashboardController(TechShareDbContext context)
        {
            _context = context;
        }

        // Luồng xử lý Dashboard giữa ng thuê và ng cho thuê
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) 
                return RedirectToAction("Login", "Auth");

            var viewModel = new DashboardViewModel();

            // Đơn đi thuê
            viewModel.MyRentals = await _context.Rentals
                .Include(r => r.Device)
                .ThenInclude(d => d.Owner)
                .Where(r => r.RenterId == userId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            // Đơn cho thuê
            viewModel.MyOrders = await _context.Rentals
                .Include(r => r.Device)
                .Include(r => r.Renter)
                .Where(r => r.Device.OwnerId == userId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(viewModel);
        }

        // Chủ máy duyệt đơn
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

        // Chủ máy -> mang máy đi giao
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Handover(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.Approved_PendingHandover)
            {
                // chờ khách xác nhận
                rental.Status = RentalStatus.PendingRenterConfirmation; 
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Khách đã test xong và xác nhận -> Bắt đầu tính 2h
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmHandover(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.PendingRenterConfirmation)
            {
                rental.Status = RentalStatus.Active;
                rental.ActualHandoverTime = DateTime.Now; 
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Khách thuê báo lỗi (chỉ dc phép trong 2h đầu)
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

        // Khách thuê trả máy
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

        // Chủ máy xác nhận đã nhận lại máy và hoàn cọc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            var rental = await _context.Rentals.Include(r => r.Device).FirstOrDefaultAsync(r => r.Id == id);
            if (rental != null && rental.Status == RentalStatus.Returned_PendingInspection)
            {
                rental.Status = RentalStatus.Completed;
                rental.DepositStatus = DepositStatus.Refunded;
                
                rental.Device.StockQuantity += rental.Quantity;

                if (rental.Device.StockQuantity > 0) 
                    rental.Device.Status = DeviceStatus.Available;
                
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
            if (rental != null && rental.Status == RentalStatus.Disputed)
            {
                rental.Status = RentalStatus.Cancelled; // Đơn bị hủy do lỗi
                rental.DepositStatus = DepositStatus.Refunded; // Trả lại cọc cho sinh viên
                
                rental.Device.StockQuantity += rental.Quantity; 

                if (rental.Device.StockQuantity > 0) 
                    rental.Device.Status = DeviceStatus.Available;
                
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
