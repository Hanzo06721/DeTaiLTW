namespace PCStore.Models;

// Bảng dbo.order_details. Đây chỉ là khung Model, chưa cấu hình truy vấn SQL.
public class OrderDetail
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
