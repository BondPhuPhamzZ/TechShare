using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TechShare.ViewModels
{
    public class DeviceCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên thiết bị")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá thuê/ngày")]
        public decimal PricePerDay { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiền cọc")]
        public decimal DepositAmount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
        public int StockQuantity { get; set; }

        public TechShare.Enums.DeviceStatus Status { get; set; }

        public string? Specifications { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
