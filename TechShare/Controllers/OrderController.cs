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
    public class OrderController : Controller
    {
        private readonly TechShareDbContext _context;

        public OrderController(TechShareDbContext context)
        {
            _context = context;
        }

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

        public async Task<IActionResult> Invoice(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Renter)
                .Include(r => r.Device)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
                return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && (string.IsNullOrEmpty(userIdStr) || rental.RenterId.ToString() != userIdStr))
            {
                return Forbid();
            }

            return View(rental);
        }
    }
}
