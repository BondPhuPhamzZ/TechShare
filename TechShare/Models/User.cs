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
        
        [MaxLength(200)]
        public string? Address { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Role { get; set; } = "User"; // User / Admin

        // Khóa tài khoản
        public bool IsLocked { get; set; } = false;

        // Xác minh danh tính (eKYC)
        public bool IsVerified { get; set; } = false;
        
        [MaxLength(500)]
        public string? CccdImageUrl { get; set; }

        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
