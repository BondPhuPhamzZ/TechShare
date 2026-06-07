using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechShare.Data;
using TechShare.Enums;
using TechShare.ViewModels;

namespace TechShare.Controllers
{
    public class DashboardController : Controller
    {
        private readonly TechShareDbContext _context;

        public DashboardController(TechShareDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int mockRenterId = 2; // Người thuê
            int mockOwnerId = 1;  // Chủ

            var viewModel = new DashboardViewModel();

            // Đơn đi thuê
            viewModel.MyRentals = await _context.Rentals
                .Include(r => r.Device)
                .ThenInclude(d => d.Owner)
                .Where(r => r.RenterId == mockRenterId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            // Đơn người != thuê máy của mình
            viewModel.MyOrders = await _context.Rentals
                .Include(r => r.Device)
                .Include(r => r.Renter)
                .Where(r => r.Device.OwnerId == mockOwnerId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(viewModel);
        }


        // Chủ máy duyệt đơn
        [HttpPost]
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

        // Chủ máy xác nhận đã giao máy -> Bắt đầu tính 2h
        [HttpPost]
        public async Task<IActionResult> Handover(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.Approved_PendingHandover)
            {
                rental.Status = RentalStatus.Active;
                rental.ActualHandoverTime = DateTime.Now; // Kích hoạt bộ đếm 2H
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Khách báo lỗi thiết bị -> Chỉ được trong 2h đầu kể từ lúc chủ máy xác nhận đã giao máy
        [HttpPost]
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

        // Khách trả máy
        [HttpPost]
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

        // Chủ máy xác nhận đã trả lại máy và hoàn cọc
        [HttpPost]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            var rental = await _context.Rentals.Include(r => r.Device).FirstOrDefaultAsync(r => r.Id == id);
            if (rental != null && rental.Status == RentalStatus.Returned_PendingInspection)
            {
                rental.Status = RentalStatus.Completed;
                rental.DepositStatus = DepositStatus.Refunded;
                
                // Trả lại số lượng tồn kho lên sàn
                rental.Device.StockQuantity += rental.Quantity;
                
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // 6. Xử lý Tranh Chấp: Chủ máy chấp nhận lỗi -> Hủy đơn & Hoàn Cọc
        [HttpPost]
        public async Task<IActionResult> ResolveDisputeRefund(int id)
        {
            var rental = await _context.Rentals.Include(r => r.Device).FirstOrDefaultAsync(r => r.Id == id);
            if (rental != null && rental.Status == RentalStatus.Disputed)
            {
                rental.Status = RentalStatus.Cancelled; // Đơn bị hủy do lỗi
                rental.DepositStatus = DepositStatus.Refunded; // Trả lại cọc cho sinh viên
                rental.Device.StockQuantity += rental.Quantity; // Cập nhật lại kho
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // 7. Xử lý Tranh Chấp: Chủ máy xác định lỗi do khách làm hỏng -> Giữ cọc
        [HttpPost]
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
