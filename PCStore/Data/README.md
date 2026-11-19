# Data (ADO.NET / SQL Server)

Thư mục quản lý kết nối SQL Server bằng `Microsoft.Data.SqlClient`.

- `ISqlConnectionFactory.cs` / `SqlConnectionFactory.cs`: tạo và mở `SqlConnection` từ `ConnectionStrings:DefaultConnection` trong `appsettings.json`.
- Đã đăng ký DI trong `Program.cs` (có thể inject `ISqlConnectionFactory` hoặc `SqlConnectionFactory` vào các Repository).

## Ví dụ sử dụng trong Repository

```csharp
using Microsoft.Data.SqlClient;
using PCStore.Data;

public class SampleRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public SampleRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CountProductsAsync(CancellationToken ct = default)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.products", conn);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
    }
}
```
