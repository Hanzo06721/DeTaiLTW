namespace PCStore.ViewModels;

// Model phục vụ GIAO DIỆN khung admin; không truy vấn SQL.
public class AdminModuleViewModel
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Symbol { get; set; } = "▦";
    public string[] Columns { get; set; } = Array.Empty<string>();
    public string? Reminder { get; set; }
}
