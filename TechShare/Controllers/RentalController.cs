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
    // Thuê thiết bị

    [Authorize]
    public class RentalController : Controller
    {
        private readonly TechShareDbContext _context;

        public RentalController(TechShareDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Checkout(TechShare.ViewModels.RentalCheckoutViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsVerified)
            {
                TempData["ErrorMessage"] = "Vui lòng xác minh danh tính (CCCD) trong Cài đặt tài khoản để thực hiện thuê thiết bị.";
                return RedirectToAction("Index", "Profile");
            }

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == model.DeviceId);

            if (device == null) 
                return NotFound();

            var today = DateTime.Now.Date;
            if (model.StartDate.Date <= today)
            {
                TempData["ErrorMessage"] = "Ngày bắt đầu thuê phải từ ngày mai trở đi để Cửa hàng kịp chuẩn bị máy.";
                return RedirectToAction("Detail", "Device", new { id = model.DeviceId });
            }

            if (model.StartDate.Date > today.AddDays(30))
            {
                TempData["ErrorMessage"] = "Bạn chỉ có thể đặt thuê trước tối đa 30 ngày.";
                return RedirectToAction("Detail", "Device", new { id = model.DeviceId });
            }

            if (model.EndDate.Date < model.StartDate.Date)
            {
                TempData["ErrorMessage"] = "Ngày trả máy không được trước Ngày nhận máy.";
                return RedirectToAction("Detail", "Device", new { id = model.DeviceId });
            }

            int rentDays = (model.EndDate - model.StartDate).Days;
            if (rentDays <= 0) 
                rentDays = 1; 

            decimal totalPrice = rentDays * device.PricePerDay * model.Quantity;

            ViewBag.RentDays = rentDays;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.StartDate = model.StartDate;
            ViewBag.EndDate = model.EndDate;
            ViewBag.Quantity = model.Quantity;

            return View(device);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout(TechShare.ViewModels.RentalCheckoutViewModel model, [FromServices] TechShare.Services.IVNPayService vnPayService)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var device = await _context.Devices.FindAsync(model.DeviceId);
            if (device == null) 
                return NotFound();

            var today = DateTime.Now.Date;
            if (model.StartDate.Date <= today || model.StartDate.Date > today.AddDays(30) || model.EndDate.Date < model.StartDate.Date)
            {
                TempData["ErrorMessage"] = "Thông tin ngày thuê không hợp lệ. Vui lòng chọn lại.";
                return RedirectToAction("Detail", "Device", new { id = model.DeviceId });
            }

            int rentDays = (model.EndDate - model.StartDate).Days;
            if (rentDays <= 0) 
                rentDays = 1;
            decimal totalPrice = rentDays * device.PricePerDay * model.Quantity;

            // Lưu tạm thông tin đơn hàng vào TempData để dùng sau khi thanh toán thành công
            TempData["PendingRental"] = System.Text.Json.JsonSerializer.Serialize(model);
            TempData["PendingTotalPrice"] = totalPrice.ToString();

            // Tạo request thanh toán VNPay
            var vnPayModel = new VNPaymentRequestModel
            {
                Amount = (double)totalPrice,
                CreatedDate = DateTime.Now,
                Description = $"Thanh toan don thue may {device.Name}",
                FullName = User.Identity.Name ?? "Khach Hang",
                OrderId = DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(10, 99).ToString()
            };

            return Redirect(vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
        }

        public async Task<IActionResult> PaymentCallBack([FromServices] TechShare.Services.IVNPayService vnPayService)
        {
            var response = vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["ErrorMessage"] = "Lỗi thanh toán VNPay hoặc bạn đã hủy giao dịch.";
                return RedirectToAction("Index", "Home");
            }

            // Thanh toán thành công, ghi nhận đơn hàng
            if (TempData["PendingRental"] == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng. Vui lòng thử lại.";
                return RedirectToAction("Index", "Home");
            }

            var model = System.Text.Json.JsonSerializer.Deserialize<TechShare.ViewModels.RentalCheckoutViewModel>(TempData["PendingRental"].ToString());
            var totalPrice = decimal.Parse(TempData["PendingTotalPrice"].ToString());
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var device = await _context.Devices.FindAsync(model.DeviceId);
            if (device == null) return NotFound();

            var rental = new Rental
            {
                DeviceId = model.DeviceId,
                RenterId = userId, 
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Quantity = model.Quantity,
                DeliveryMethod = model.DeliveryMethod,
                DeliveryAddress = model.DeliveryAddress,
                TotalPrice = totalPrice,
                DepositStatus = DepositStatus.DaThanhToan, 
                Status = RentalStatus.ChoDuyet 
            };

            device.StockQuantity -= model.Quantity;

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thanh toán thành công! Mã giao dịch VNPay: {response.TransactionId}";
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
