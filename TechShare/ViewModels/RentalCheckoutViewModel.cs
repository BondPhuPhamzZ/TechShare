using System;
using System.ComponentModel.DataAnnotations;
using TechShare.Enums;

namespace TechShare.ViewModels
{
    public class RentalCheckoutViewModel
    {
        [Required]
        public int DeviceId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận máy.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả máy.")]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }

        public string? DeliveryAddress { get; set; }

        [Required]
        public DeliveryMethod DeliveryMethod { get; set; }
    }
}
