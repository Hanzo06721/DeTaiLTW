namespace PCStore.Models;

// Bảng dbo.product_attributes. Đây chỉ là khung Model, chưa cấu hình truy vấn SQL.
public class ProductAttribute
{
    public long ProductId { get; set; }
    public int AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
}
