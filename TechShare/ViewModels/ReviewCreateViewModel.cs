using System.ComponentModel.DataAnnotations;

namespace TechShare.ViewModels
{
    public class ReviewCreateViewModel
    {
        [Required]
        public int RentalId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá.")]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự.")]
        public string? Comment { get; set; }
    }
}
