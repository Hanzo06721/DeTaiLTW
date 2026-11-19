namespace PCStore.Models;

// Bảng dbo.users. Đây chỉ là khung Model, chưa cấu hình truy vấn SQL.
public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // SQL column: password (lưu hash, không lưu mật khẩu thô)
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
