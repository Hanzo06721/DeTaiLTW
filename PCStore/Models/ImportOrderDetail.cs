namespace PCStore.Models;

// Bảng dbo.import_order_details. Đây chỉ là khung Model, chưa cấu hình truy vấn SQL.
public class ImportOrderDetail
{
    public long Id { get; set; }
    public long ImportOrderId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal ImportPrice { get; set; }
}
