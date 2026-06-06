using System.ComponentModel.DataAnnotations;

namespace TechShare.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        
        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
