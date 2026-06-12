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
        
        public float ReputationScore { get; set; } = 5.0f;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Role { get; set; } = "User"; // User / Admin

        // Khóa tài khoản
        public bool IsLocked { get; set; } = false;

        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
