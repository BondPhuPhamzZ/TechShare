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
        
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }
        
        [MaxLength(20)]
        public string? StudentId { get; set; }
        
        public float ReputationScore { get; set; } = 5.0f;

        // Navigation properties
        public ICollection<Device> OwnedDevices { get; set; } = new List<Device>();
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
