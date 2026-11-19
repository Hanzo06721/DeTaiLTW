namespace PCStore.ViewModels.Sales;

/// <summary>
/// Hợp đồng dữ liệu giỏ hàng lưu trong Session (Key: "PCStore.Cart")
/// dùng chung giữa module Build PC (NV3) và Giỏ hàng / Đặt hàng (NV4).
/// </summary>
public class CartSessionItem
{
    public long ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Thumbnail { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public int StockQuantity { get; set; }
    public string Source { get; set; } = "CATALOG";

    public decimal LineTotal => UnitPrice * Quantity;
}
