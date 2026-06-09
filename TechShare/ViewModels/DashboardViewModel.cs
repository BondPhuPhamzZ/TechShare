using System.Collections.Generic;
using TechShare.Models;

namespace TechShare.ViewModels
{
    public class DashboardViewModel
    {
        // Danh sách đơn User ĐI THUÊ
        public List<Rental> MyRentals { get; set; } = new List<Rental>();
        
        // Danh sách đơn User CHO THUÊ
        public List<Rental> MyOrders { get; set; } = new List<Rental>();
    }
}
