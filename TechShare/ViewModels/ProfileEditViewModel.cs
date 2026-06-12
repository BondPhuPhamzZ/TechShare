using System.ComponentModel.DataAnnotations;

namespace TechShare.ViewModels
{
    public class ProfileEditViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }
    }
}
