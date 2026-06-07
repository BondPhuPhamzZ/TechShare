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
            // Truy vấn lấy thiết bị kèm Chủ máy, Danh mục và toàn bộ Đơn thuê -> Đánh giá (Reviews)
            var device = await _context.Devices
                .Include(d => d.Owner)
                .Include(d => d.Category)
                .Include(d => d.Rentals)
                    .ThenInclude(r => r.Reviews)
                        .ThenInclude(rev => rev.Reviewer) // Lấy thông tin người đánh giá
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }
    }
}
