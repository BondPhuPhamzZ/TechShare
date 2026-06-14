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
    public class OrderController : Controller
    {
        private readonly TechShareDbContext _context;

        public OrderController(TechShareDbContext context)
        {
            _context = context;
        }

        // Lịch sử / Tiến độ các thiết bị mình đang thuê
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) 
                return RedirectToAction("Login", "Auth");

            var viewModel = new OrderViewModel();

            viewModel.ActiveRentals = await _context.Rentals
                .Include(r => r.Device)
                .Where(r => r.RenterId == userId && (
                    r.Status == RentalStatus.ChoDuyet || 
                    r.Status == RentalStatus.DaDuyet || 
                    r.Status == RentalStatus.DangGiao || 
                    r.Status == RentalStatus.DangThue || 
                    r.Status == RentalStatus.TranhChap ||
                    r.Status == RentalStatus.ChoTra
                ))
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            viewModel.CompletedRentals = await _context.Rentals
                .Include(r => r.Device)
                .Where(r => r.RenterId == userId && (r.Status == RentalStatus.HoanTat || r.Status == RentalStatus.DaHuy))
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(viewModel); 
        }



        // Khách thuê báo lỗi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportIssue(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null && rental.Status == RentalStatus.DangThue)
            {
                rental.Status = RentalStatus.TranhChap;
                await _context.SaveChangesAsync();
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
