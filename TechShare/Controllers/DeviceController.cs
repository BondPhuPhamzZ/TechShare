using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShare.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechShare.ViewModels;
using System.Security.Claims;
using TechShare.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using TechShare.Enums;

namespace TechShare.Controllers
{
    public class DeviceController : Controller
    {
        private readonly TechShareDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DeviceController(TechShareDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Trang chi tiết của thiết bị
        public async Task<IActionResult> Detail(int id)
        {
            var device = await _context.Devices
                .Include(d => d.Category)
                .Include(d => d.Rentals)
                    .ThenInclude(r => r.Review)
                        .ThenInclude(rev => rev.Reviewer) 
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }

        public async Task<IActionResult> Catalog(string q, int? categoryId, int page = 1)
        {
            var query = _context.Devices
                .Include(d => d.Category) 
                .Include(d => d.Rentals)
                    .ThenInclude(r => r.Review)
                .Where(d => d.Status == DeviceStatus.SanSang && d.StockQuantity > 0);

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(d => d.Name.Contains(q));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            int pageSize = 8;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var devices = await query
                .OrderByDescending(d => d.Id) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["SearchQuery"] = q;
            ViewData["CategoryId"] = categoryId;
            ViewData["Categories"] = await _context.Categories.ToListAsync();
            
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(devices);
        }

        // Xử lý Thêm thiết bị mới (Từ Modal)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeviceCreateViewModel model)
        {
            if (model.ImageFile == null)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ảnh thiết bị!";
                return RedirectToAction("Devices", "Admin");
            }

            if (model.PricePerDay <= 0 || model.DepositAmount <= 0)
            {
                TempData["ErrorMessage"] = "Giá thuê và Tiền cọc phải lớn hơn 0!";
                return RedirectToAction("Devices", "Admin");
            }

            if (model.StockQuantity < 0)
            {
                TempData["ErrorMessage"] = "Tồn kho không được âm!";
                return RedirectToAction("Devices", "Admin");
            }

            if (ModelState.IsValid)
            {
                string uniqueFileName = "";

                // Kiểm tra định dạng đuôi file ảnh
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "Chỉ hỗ trợ file ảnh (.jpg, .jpeg, .png, .gif)";
                    return RedirectToAction("Devices", "Admin");
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                // Tạo thông tin thiết bị lưu vào DB
                var device = new Device()
                {
                    Name = model.Name,
                    CategoryId = model.CategoryId,
                    PricePerDay = model.PricePerDay,
                    DepositAmount = model.DepositAmount,
                    StockQuantity = model.StockQuantity,
                    Description = model.Description,
                    Specifications = model.Specifications,
                    Status = model.Status,
                    ImageUrl = "/images/" + uniqueFileName
                };

                _context.Devices.Add(device);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm thiết bị mới thành công!";
                return RedirectToAction("Devices", "Admin");
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ!";
            return RedirectToAction("Devices", "Admin");
        }

        // Xử lý Cập nhật thiết bị (Từ Modal)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DeviceCreateViewModel model)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thiết bị!";
                return RedirectToAction("Devices", "Admin");
            }

            if (model.PricePerDay <= 0 || model.DepositAmount <= 0)
            {
                TempData["ErrorMessage"] = "Giá thuê và Tiền cọc phải lớn hơn 0!";
                return RedirectToAction("Devices", "Admin");
            }

            if (model.StockQuantity < 0)
            {
                TempData["ErrorMessage"] = "Tồn kho không được âm!";
                return RedirectToAction("Devices", "Admin");
            }

            if (ModelState.IsValid)
            {
                if (model.ImageFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["ErrorMessage"] = "Chỉ hỗ trợ file ảnh (.jpg, .jpeg, .png, .gif)";
                        return RedirectToAction("Devices", "Admin");
                    }

                    string uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                    device.ImageUrl = "/images/" + uniqueFileName;
                }

                device.Name = model.Name;
                device.CategoryId = model.CategoryId;
                device.PricePerDay = model.PricePerDay;
                device.DepositAmount = model.DepositAmount;
                device.StockQuantity = model.StockQuantity;
                device.Description = model.Description;
                device.Specifications = model.Specifications;
                device.Status = model.Status;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thiết bị thành công!";
                return RedirectToAction("Devices", "Admin");
            }

            TempData["ErrorMessage"] = "Dữ liệu cập nhật không hợp lệ!";
            return RedirectToAction("Devices", "Admin");
        }

        // Xóa (Ẩn) thiết bị
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device != null)
            {
                device.Status = DeviceStatus.BaoTri;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã tạm ẩn thiết bị (Chuyển sang trạng thái Bảo trì)!";
            }
            return RedirectToAction("Devices", "Admin");
        }
    }
}
