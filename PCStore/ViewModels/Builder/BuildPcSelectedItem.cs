namespace PCStore.ViewModels.Builder;

public class BuildPcSelectedItem
{
    public string SlotKey { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
