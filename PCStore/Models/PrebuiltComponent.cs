namespace PCStore.Models;

// Bảng dbo.prebuilt_components. Đây chỉ là khung Model, chưa cấu hình truy vấn SQL.
public class PrebuiltComponent
{
    public long PcId { get; set; }
    public long ComponentId { get; set; }
    public int Quantity { get; set; } = 1;
}
