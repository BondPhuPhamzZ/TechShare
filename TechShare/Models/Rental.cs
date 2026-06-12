using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechShare.Enums;

namespace TechShare.Models
{
    public class Rental
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }
        

        public int Quantity { get; set; } = 1; 
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; } 
        
        public RentalStatus Status { get; set; } = RentalStatus.ChoDuyet;

        // Trạng thái tiền cọc
        public DepositStatus DepositStatus { get; set; } = DepositStatus.ChuaThanhToan;

        // Phương thức giao hàng
        public DeliveryMethod DeliveryMethod { get; set; } = DeliveryMethod.TuLay;
        
        [MaxLength(500)]
        public string? DeliveryAddress { get; set; } // Nếu GiaoTanNoi thì lưu địa chỉ khách

        // Đánh giá
        public bool IsReviewed { get; set; } = false;

        // Foreign Keys
        public int DeviceId { get; set; }
        public Device Device { get; set; } = null!;

        public int RenterId { get; set; }
        public User Renter { get; set; } = null!;
        
        // 1 Rental có 1 Review
        public Review? Review { get; set; } 
    }
}
