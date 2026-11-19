namespace PCStore.ViewModels.Builder;

public class BuildPcSlotRowViewModel
{
    public BuildPcSlotDefinition Slot { get; set; } = new();
    public BuildPcProductDto? SelectedProduct { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => SelectedProduct is null ? 0m : SelectedProduct.Price * Quantity;
    public int AvailableProductsCount { get; set; }
    public string? StockNote { get; set; }
}

public class BuildPcPageViewModel
{
    public List<BuildPcSlotRowViewModel> Rows { get; set; } = new();
    public CompatibilityReport Compatibility { get; set; } = new();

    public int SelectedCount => Rows.Count(r => r.SelectedProduct is not null);
    public int TotalSlots => Rows.Count;
    public int CoreSelectedCount => Rows.Count(r => r.Slot.IsCore && r.SelectedProduct is not null);
    public int CoreTotalSlots => Rows.Count(r => r.Slot.IsCore);
    public int CompletionPercent => CoreTotalSlots == 0 ? 0 : (int)Math.Round(CoreSelectedCount * 100.0 / CoreTotalSlots);

    public int TotalItemsQuantity => Rows.Where(r => r.SelectedProduct is not null).Sum(r => r.Quantity);
    public decimal TotalPrice => Rows.Sum(r => r.LineTotal);
    public bool HasItems => SelectedCount > 0;
    public int CartCount { get; set; }
}
