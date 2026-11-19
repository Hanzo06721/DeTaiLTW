namespace PCStore.Models;

// Bảng dbo.products. Đây chỉ là khung Model, chưa cấu hình truy vấn SQL.
public class Product
{
    public long Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Thumbnail { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
}
