using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechShare.Enums;

namespace TechShare.Models
{
    public class Device
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;
        
        public string? Description { get; set; }
        
        public string? ImageUrl { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositAmount { get; set; } 
        
        public int StockQuantity { get; set; } = 1; 
        
        public DeviceStatus Status { get; set; } = DeviceStatus.Available;

        // Foreign Keys
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        // Navigation Property: Một thiết bị có thể có nhiều đơn thuê
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
