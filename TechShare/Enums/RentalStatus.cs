namespace TechShare.Enums
{
    public enum RentalStatus
    {
        Pending = 0,               // Chờ duyệt
        Approved_PendingHandover = 1, // Đã duyệt, hẹn ngày giao máy
        Active = 2,                // Khách ĐÃ NHẬN MÁY (Bắt đầu đếm ngược 2H)
        Returned_PendingInspection = 3, // Khách trả máy, chờ kiểm tra
        Completed = 4,             // Hoàn thành, hoàn cọc
        Disputed = 5,              // Tranh chấp / Báo hỏng trong 2H đầu
        Cancelled = 6              // Hủy đơn
    }
}
