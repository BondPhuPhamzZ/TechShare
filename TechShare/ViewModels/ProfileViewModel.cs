using System.Collections.Generic;
using TechShare.Models;

namespace TechShare.ViewModels
{
    public class ProfileViewModel
    {
        public User UserInfo { get; set; }
        
        // Danh sách thiết bị bạn mang lên sàn cho thuê
        public List<Device> MyPostedDevices { get; set; } = new List<Device>();
        
        // Danh sách thiết bị bạn ĐANG thuê (Đang cầm trên tay)
        public List<Rental> MyActiveRentals { get; set; } = new List<Rental>();
    }
}
