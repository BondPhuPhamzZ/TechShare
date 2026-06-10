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
    // Trạng thái/ tiến độ thuê thiết bị

    [Authorize]
    public class RentalHistoryController : Controller
    {
        private readonly TechShareDbContext _context;

        public RentalHistoryController(TechShareDbContext context)
        {
            _context = context;
        }

        // Lịch sử / Tiến độ các thiết bị mình đang thuê
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) 
                return RedirectToAction("Login", "Auth");

            var viewModel = new RentalHistoryViewModel();

            viewModel.ActiveRentals = await _context.Rentals
                .Include(r => r.Device)
                .ThenInclude(d => d.Owner)
                .Where(r => r.RenterId == userId && (r.Status == RentalStatus.DangThue || r.Status == RentalStatus.ChoGiao || r.Status == RentalStatus.TranhChap || r.Status == RentalStatus.DangCheck))
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            viewModel.PendingRentals = await _context.Rentals
                .Include(r => r.Device)
                .ThenInclude(d => d.Owner)
                .Where(r => r.RenterId == userId && r.Status == RentalStatus.ChoDuyet)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            viewModel.CompletedRentals = await _context.Rentals
                .Include(r => r.Device)
                .ThenInclude(d => d.Owner)
                .Where(r => r.RenterId == userId && (r.Status == RentalStatus.HoanTat || r.Status == RentalStatus.DaHuy || r.Status == RentalStatus.ChoTra))
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(viewModel); 
        }

        // Khách đã test xong và xác nhận -> Bắt đầu tính 2h
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmHandover(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.DangCheck)
            {
                rental.Status = RentalStatus.DangThue;
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
            // Chỉ được báo lỗi nếu trạng thái là DangThue và chưa quá 2 tiếng từ lúc bàn giao
            if (rental != null && rental.Status == RentalStatus.DangThue)
            {
                if (rental.ActualHandoverTime.HasValue && (DateTime.Now - rental.ActualHandoverTime.Value).TotalHours <= 2)
                {
                    rental.Status = RentalStatus.TranhChap;
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
            if (rental != null && rental.Status == RentalStatus.DangThue)
            {
                rental.Status = RentalStatus.ChoTra;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
