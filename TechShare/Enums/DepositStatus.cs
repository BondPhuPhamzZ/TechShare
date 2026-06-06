namespace TechShare.Enums
{
    public enum DepositStatus
    {
        Pending = 0, 
        Paid = 1, 
        Refunded = 2, 
        Retained = 3 // chủ máy sẽ giữ lại cọc (tiền đền) -> Nếu người thuê làm hỏng
    }
}
