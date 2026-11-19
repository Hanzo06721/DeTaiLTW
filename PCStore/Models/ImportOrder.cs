namespace PCStore.Models;

// Bảng dbo.import_orders. Đây chỉ là khung Model, chưa cấu hình truy vấn SQL.
public class ImportOrder
{
    public long Id { get; set; }
    public int SupplierId { get; set; }
    public long UserId { get; set; }
    public DateTime ImportDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
}
