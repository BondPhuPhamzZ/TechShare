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

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var model = new ProfileEditViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ProfileEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không chính xác.");
                return View(model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index");
        }

        // Xác minh CCCD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCccd(Microsoft.AspNetCore.Http.IFormFile cccdImage)
        {
            if (cccdImage == null || cccdImage.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ảnh hợp lệ.";
                return RedirectToAction("Index");
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            string uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "images", "cccd");
            if (!System.IO.Directory.Exists(uploadsFolder))
            {
                System.IO.Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = System.Guid.NewGuid().ToString() + "_" + cccdImage.FileName;
            string filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await cccdImage.CopyToAsync(fileStream);
            }

            user.CccdImageUrl = "/images/cccd/" + uniqueFileName;
            user.IsVerified = false; 
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tải ảnh CCCD thành công. Vui lòng chờ cửa hàng duyệt.";
            return RedirectToAction("Index");
        }
    }
}
