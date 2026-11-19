namespace PCStore.Models;

// Bảng dbo.orders. Đây chỉ là khung Model, chưa cấu hình truy vấn SQL.
public class Order
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string? ReceiverEmail { get; set; }
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string AddressDetail { get; set; } = string.Empty;
    public string? OrderNote { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
}
