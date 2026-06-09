using System.Collections.Generic;
using TechShare.Models;

namespace TechShare.ViewModels
{
    public class ProfileViewModel
    {
        public User UserInfo { get; set; }
        public List<Device> MyPostedDevices { get; set; } = new List<Device>();
        public List<Rental> MyActiveRentals { get; set; } = new List<Rental>();
    }
}
