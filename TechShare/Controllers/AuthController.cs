using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TechShare.Data;
using TechShare.Models;
using TechShare.ViewModels;

namespace TechShare.Controllers
{
    public class AuthController : Controller
    {
        private readonly TechShareDbContext _context;

        public AuthController(TechShareDbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            // Trả về giao diện đăng nhập cho người dùng
            return View(new LoginViewModel());
        }

        // POST: Xử lý Đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken] // CHỐNG CSRF: Bắt buộc form gửi lên phải có thẻ ẩn tương ứng
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra DB xem user có tồn tại không
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == model.Password);
                
                if (user != null)
                {
                    // Đăng nhập thành công -> Tạo thẻ chứng minh thư (Claims)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);

                    // Đóng dấu cấp Cookie cho trình duyệt
                    await HttpContext.SignInAsync("Cookies", principal);

                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // GET: /Auth/Register
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: Xử lý Đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email trùng
                bool isExist = await _context.Users.AnyAsync(u => u.Email == model.Email);
                if (isExist)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                var newUser = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Username = model.Email, // [FIX LỖI DB]: Cột Username là Required nhưng trước đó chưa được gán
                    PhoneNumber = model.PhoneNumber,
                    // THỰC TẾ: Phải băm mật khẩu (Hash). Trong đồ án demo ta lưu text tĩnh.
                    PasswordHash = model.Password, 
                    ReputationScore = 5.0f // Tặng 5 sao khởi điểm cho tài khoản mới
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Đăng ký xong tự động đăng nhập luôn
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, newUser.FullName),
                    new Claim(ClaimTypes.Email, newUser.Email)
                };
                var identity = new ClaimsIdentity(claims, "Cookies");
                await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(identity));

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // Đăng xuất
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }
    }
}
