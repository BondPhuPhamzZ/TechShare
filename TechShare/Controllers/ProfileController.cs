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

    }
}
