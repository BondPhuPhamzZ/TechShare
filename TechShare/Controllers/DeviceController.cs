using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShare.Data;
using System.Threading.Tasks;

namespace TechShare.Controllers
{
    public class DeviceController : Controller
    {
        private readonly TechShareDbContext _context;

        public DeviceController(TechShareDbContext context)
        {
            _context = context;
        }

        // Dùng chuẩn Async/Await cho tính năng Chi tiết thiết bị
        public async Task<IActionResult> Detail(int id)
        {
            // Truy vấn lấy thiết bị kèm Chủ máy và Danh mục
            var device = await _context.Devices
                .Include(d => d.Owner)
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }
    }
}
