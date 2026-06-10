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
                .ThenInclude(d => d.Owner)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null || rental.Status != RentalStatus.HoanTat)
                return NotFound();

            bool hasReviewed = await _context.Reviews.AnyAsync(r => r.RentalId == rentalId);
            if (hasReviewed)
            {
                return RedirectToAction("Index", "RentalHistory");
            }

            return View(rental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int rentalId, int rating, string comment)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var rental = await _context.Rentals
                .Include(r => r.Device)
                .ThenInclude(d => d.Owner)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null) 
                return NotFound();

            var review = new Review
            {
                RentalId = rentalId,
                Rating = rating,
                Comment = comment,
                ReviewerId = userId, 
                RevieweeId = rental.Device.OwnerId 
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync(); // Lưu Reviews vào data -> Tính trung bình
            
            // Tính TB 
            var allReviews = await _context.Reviews
                .Where(r => r.RevieweeId == rental.Device.OwnerId)
                .ToListAsync();

            float average = (float)allReviews.Average(r => r.Rating);
            
            // Cập nhật điểm cho Chủ thiết bị và đánh dấu đơn đã review
            rental.Device.Owner.ReputationScore = average;
            rental.IsReviewed = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "RentalHistory");
        }
    }
}
