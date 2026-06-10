using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TechShare.Data;
using TechShare.Enums;

namespace TechShare.Controllers
{
    [Authorize]
    public class RentalHistoryController : Controller
    {
        private readonly TechShareDbContext _context;

        public RentalHistoryController(TechShareDbContext context)
        {
            _context = context;
        }

        // Lịch sử / Tiến độ các thiết bị mình ĐANG ĐI THUÊ
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) 
                return RedirectToAction("Login", "Auth");

            var myRentals = await _context.Rentals
                .Include(r => r.Device)
                .ThenInclude(d => d.Owner)
                .Where(r => r.RenterId == userId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(myRentals); // Truyền thẳng List thay vì ViewModel
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
    }
}
