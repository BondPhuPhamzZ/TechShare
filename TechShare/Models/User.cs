using System.ComponentModel.DataAnnotations;

namespace TechShare.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;
        
        [Required]
        public string PasswordHash { get; set; } = null!;
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;
        
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;
        
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }
        
        [MaxLength(20)]
        public string? StudentId { get; set; }
        
        public float ReputationScore { get; set; } = 5.0f;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Role { get; set; } = "User"; // User / Admin

        // Thông tin xác thực (KYC)
        public bool IsVerified { get; set; } = false;
        
        [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD phải gồm đúng 12 chữ số")]
        public string? IdCardNumber { get; set; }

        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
