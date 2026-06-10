namespace TechShare.Enums
{
    public enum RentalStatus
    {
        ChoDuyet = 0,      // Pending
        ChoGiao = 1,       // Approved_PendingHandover
        DangCheck = 2,     // PendingRenterConfirmation
        DangThue = 3,      // Active
        ChoTra = 4,        // Returned_PendingInspection
        HoanTat = 5,       // Completed
        TranhChap = 6,     // Disputed
        DaHuy = 7          // Cancelled
    }
}
