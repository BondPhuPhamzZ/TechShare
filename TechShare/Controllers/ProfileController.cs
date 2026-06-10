using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TechShare.Data;
using TechShare.Enums;
using TechShare.ViewModels;

namespace TechShare.Controllers
{
    [Authorize] 
    public class ProfileController : Controller
    {
        private readonly TechShareDbContext _context;

        public ProfileController(TechShareDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var viewModel = new ProfileViewModel
            {
                UserInfo = await _context.Users.FindAsync(userId)
            };

            return View(viewModel);
        }

        // --- Xác thực định danh (KYC) ---
        [HttpGet]
        public async Task<IActionResult> VerifyIdentity()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.IsVerified) return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyIdentity(string idCardNumber)
        {
            if (string.IsNullOrWhiteSpace(idCardNumber) || idCardNumber.Length != 12)
            {
                ModelState.AddModelError("", "CCCD phải gồm đúng 12 chữ số.");
                return View();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IdCardNumber = idCardNumber;
                user.IsVerified = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // --- Xem Hồ sơ công khai ---
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _context.Users
                .Include(u => u.Devices.Where(d => d.Status == DeviceStatus.SanSang && d.StockQuantity > 0))
                .FirstOrDefaultAsync(u => u.Id == id);
                
            if (user == null) return NotFound();

            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.RevieweeId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Reviews = reviews;

            return View(user);
        }
    }
}
