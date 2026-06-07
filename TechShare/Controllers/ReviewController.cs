using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShare.Data;
using TechShare.Models;
using TechShare.Enums;
using System.Threading.Tasks;
using System.Linq;

namespace TechShare.Controllers
{
    public class ReviewController : Controller
    {
        private readonly TechShareDbContext _context;

        public ReviewController(TechShareDbContext context)
        {
            _context = context;
        }

        // GET: Hiện form Đánh giá
        public async Task<IActionResult> Create(int rentalId)
        {
            var rental = await _context.Rentals
                .Include(r => r.Device)
                .ThenInclude(d => d.Owner)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null || rental.Status != RentalStatus.Completed) 
                return NotFound();

            // Kiểm tra xem đơn này đã được đánh giá chưa để tránh trùng lặp
            bool hasReviewed = await _context.Reviews.AnyAsync(r => r.RentalId == rentalId);
            if (hasReviewed)
            {
                // Nếu đánh giá rồi thì về trang Dashboard
                return RedirectToAction("Index", "Dashboard");
            }

            return View(rental);
        }

        // POST: Xử lý lưu Đánh giá và Tính lại Uy tín cho Chủ máy
        [HttpPost]
        public async Task<IActionResult> SubmitReview(int rentalId, int rating, string comment)
        {
            var rental = await _context.Rentals
                .Include(r => r.Device)
                .ThenInclude(d => d.Owner)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null) return NotFound();

            // Tạo bản ghi Review
            var review = new Review
            {
                RentalId = rentalId,
                Rating = rating,
                Comment = comment,
                ReviewerId = 2, // Hardcode Khách Thuê ID = 2
                RevieweeId = rental.Device.OwnerId // Đánh giá Chủ máy
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync(); // Lưu Review trước để có data tính trung bình

            // Tính trung bình cộng điểm uy tín của Chủ máy
            var allReviews = await _context.Reviews
                .Where(r => r.RevieweeId == rental.Device.OwnerId)
                .ToListAsync();

            float average = (float)allReviews.Average(r => r.Rating);
            
            // Cập nhật điểm cho Chủ máy
            rental.Device.Owner.ReputationScore = average;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
