using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShare.Data;
using TechShare.Models;
using TechShare.Enums;
using System.Threading.Tasks;
using System.Linq;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TechShare.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly TechShareDbContext _context;

        public ReviewController(TechShareDbContext context)
        {
            _context = context;
        }

        // Form đánh giá
        public async Task<IActionResult> Create(int rentalId)
        {
            var rental = await _context.Rentals
                .Include(r => r.Device)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null || rental.Status != RentalStatus.HoanTat)
                return NotFound();

            bool hasReviewed = await _context.Reviews.AnyAsync(r => r.RentalId == rentalId);
            if (hasReviewed)
            {
                return RedirectToAction("Index", "Order");
            }

            return View(rental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(TechShare.ViewModels.ReviewCreateViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var rental = await _context.Rentals
                .Include(r => r.Device)
                .FirstOrDefaultAsync(r => r.Id == model.RentalId);

            if (rental == null) 
                return NotFound();

            var review = new Review
            {
                RentalId = model.RentalId,
                Rating = model.Rating,
                Comment = model.Comment,
                ReviewerId = userId
            };

            _context.Reviews.Add(review);
            rental.IsReviewed = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Order");
        }
    }
}
