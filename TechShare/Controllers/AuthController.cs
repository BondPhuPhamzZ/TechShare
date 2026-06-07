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

        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == model.Password);
                
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("Cookies", principal);

                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
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
                    Username = model.Email, 
                    PhoneNumber = model.PhoneNumber,
                    PasswordHash = model.Password, 
                    ReputationScore = 5.0f 
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Login", "Home", model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }
    }
}
