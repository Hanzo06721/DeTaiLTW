namespace PCStore.Models;

// Bảng dbo.categories. Đây chỉ là khung Model, chưa cấu hình truy vấn SQL.
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}
