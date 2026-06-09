namespace TechShare.Enums
{
    public enum RentalStatus
    {
        Pending = 0,               // Chờ duyệt
        Approved_PendingHandover = 1, // Đã duyệt, hẹn ngày giao máy
        PendingRenterConfirmation = 2, // Khách đang cầm máy để test (chờ khách xác nhận)
        Active = 3,                // Khách đã nhận máy (Bắt đầu đếm ngược 2H)
        Returned_PendingInspection = 4, // Khách trả máy, chờ kiểm tra
        Completed = 5,             // Hoàn thành, hoàn cọc
        Disputed = 6,              // Tranh chấp / Báo hỏng trong 2H đầu
        Cancelled = 7              // Hủy đơn
    }
}
