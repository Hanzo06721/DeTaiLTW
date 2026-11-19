namespace PCStore.Models;

// Bảng dbo.roles. Đây chỉ là khung Model, chưa cấu hình truy vấn SQL.
public class Role
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
