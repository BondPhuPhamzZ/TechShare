using System.Collections.Generic;
using TechShare.Models;

namespace TechShare.ViewModels
{
    public class HostViewModel
    {
        // Danh sách thiết bị mình đăng cho thuê
        public IEnumerable<Device> MyPostedDevices { get; set; }
        
        // Danh sách các đơn khách đang đặt thuê máy của mình
        public IEnumerable<Rental> MyOrders { get; set; }
    }
}
