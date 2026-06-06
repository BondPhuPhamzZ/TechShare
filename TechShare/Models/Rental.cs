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
        
        // Thời điểm chủ máy giao thiết bị (đếm ngược 2H)
        public DateTime? ActualHandoverTime { get; set; } 
        
        public int Quantity { get; set; } = 1; 
        
        public DeliveryMethod DeliveryMethod { get; set; }
        
        [MaxLength(500)]
        public string? DeliveryAddress { get; set; } 
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; } 
        
        public DepositStatus DepositStatus { get; set; } = DepositStatus.Pending;
        
        public RentalStatus Status { get; set; } = RentalStatus.Pending;

        // Foreign Keys
        public int DeviceId { get; set; }
        public Device Device { get; set; } = null!;

        public int RenterId { get; set; }
        public User Renter { get; set; } = null!;
        
        // 1 Rental có 1 Review
        public Review? Review { get; set; } 
    }
}
