using System.ComponentModel.DataAnnotations;

namespace TechShare.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        
        [Range(1, 5)]
        public int Rating { get; set; } 
        
        [MaxLength(1000)]
        public string? Comment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Keys
        public int RentalId { get; set; }
        public Rental Rental { get; set; } = null!;

        public int ReviewerId { get; set; }
        public User Reviewer { get; set; } = null!;

    }
}
