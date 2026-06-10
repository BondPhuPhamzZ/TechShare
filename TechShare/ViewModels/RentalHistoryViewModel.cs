using TechShare.Models;

namespace TechShare.ViewModels
{
    public class RentalHistoryViewModel
    {
        public List<Rental> ActiveRentals { get; set; } = new List<Rental>();
        public List<Rental> PendingRentals { get; set; } = new List<Rental>();
        public List<Rental> CompletedRentals { get; set; } = new List<Rental>();
    }
}
