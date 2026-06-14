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

        public async Task<IActionResult> Catalog(string q, int? categoryId)
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

            var devices = await query
                .OrderByDescending(d => d.Id) 
                .ToListAsync();

            ViewData["SearchQuery"] = q;
            ViewData["CategoryId"] = categoryId;
            ViewData["Categories"] = await _context.Categories.ToListAsync();

            return View(devices);
        }

        // Trang upload thiết bị
        [Authorize(Roles = "Admin")]
        // GET: Device/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // Xử lý đăng thiết bị cho thuê lên
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeviceCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = "";

                if (model.ImageFile != null)
                {
                    // Kiểm tra định dạng đuôi file ảnh
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Chỉ hỗ trợ file ảnh (.jpg, .jpeg, .png, .gif)");
                        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
                        TempData["ErrorMessage"] = "Đăng bài thất bại: Sai định dạng ảnh!";
                        return View(model);
                    }

                    string uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
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
                    ImageUrl = "/images/" + uniqueFileName
                };

                _context.Devices.Add(device);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng bài thành công!";
                return RedirectToAction("Devices", "Admin");
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View(model);
        }

        // Chỉnh sửa thiết bị (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            var model = new DeviceCreateViewModel
            {
                Name = device.Name,
                CategoryId = device.CategoryId,
                PricePerDay = device.PricePerDay,
                DepositAmount = device.DepositAmount,
                StockQuantity = device.StockQuantity,
                Description = device.Description
                // ImageUrl will be kept if ImageFile is null
            };
            ViewBag.CurrentImageUrl = device.ImageUrl;
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", device.CategoryId);
            return View(model);
        }

        // Xử lý chỉnh sửa thiết bị
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DeviceCreateViewModel model)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (model.ImageFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Chỉ hỗ trợ file ảnh (.jpg, .jpeg, .png, .gif)");
                        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
                        ViewBag.CurrentImageUrl = device.ImageUrl;
                        return View(model);
                    }

                    string uploadsFolder = Path.Combine(_env.WebRootPath, "images");
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

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thiết bị thành công!";
                return RedirectToAction("Devices", "Admin");
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            ViewBag.CurrentImageUrl = device.ImageUrl;
            return View(model);
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
