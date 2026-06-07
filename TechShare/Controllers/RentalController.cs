using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShare.Data;
using TechShare.Enums;
using TechShare.Models;
using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TechShare.Controllers
{
    [Authorize]
    public class RentalController : Controller
    {
        private readonly TechShareDbContext _context;

        public RentalController(TechShareDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Checkout(int deviceId, DateTime startDate, DateTime endDate, int quantity)
        {
            var device = await _context.Devices
                .Include(d => d.Owner)
                .FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null) 
                return NotFound();

            // Tính số ngày và tổng tiền
            int rentDays = (endDate - startDate).Days;
            if (rentDays <= 0) 
                rentDays = 1; // Ít nhất 1 ngày

            decimal totalPrice = rentDays * device.PricePerDay * quantity;

            ViewBag.RentDays = rentDays;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Quantity = quantity;

            return View(device);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout(int deviceId, DateTime startDate, DateTime endDate, int quantity, string deliveryAddress, DeliveryMethod deliveryMethod)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null) 
                return NotFound();

            int rentDays = (endDate - startDate).Days;
            if (rentDays <= 0) rentDays = 1;
            decimal totalPrice = rentDays * device.PricePerDay * quantity;

            var rental = new Rental
            {
                DeviceId = deviceId,
                RenterId = userId, // Đã lấy từ ID thật đăng nhập
                StartDate = startDate,
                EndDate = endDate,
                Quantity = quantity,
                DeliveryMethod = deliveryMethod,
                DeliveryAddress = deliveryAddress,
                TotalPrice = totalPrice,
                DepositStatus = DepositStatus.Paid, // Giả lập đã thanh toán cọc qua Momo/VNPay thành công
                Status = RentalStatus.Pending // Chờ chủ máy duyệt
            };

            // Trừ số lượng tồn kho của thiết bị ngay lập tức để tránh người khác thuê mất
            device.StockQuantity -= quantity;

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { id = rental.Id });
        }

        public async Task<IActionResult> Success(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Device)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (rental == null) 
                return NotFound();

            return View(rental);
        }
    }
}
