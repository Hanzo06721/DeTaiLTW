namespace PCStore.Models;

// Bảng dbo.suppliers. Đây chỉ là khung Model, chưa cấu hình truy vấn SQL.
public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
}
