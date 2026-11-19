namespace PCStore.ViewModels.Builder;

public enum CompatibilitySeverity
{
    Ok,
    Info,
    Warning,
    Error
}

public class CompatibilityIssue
{
    public string Code { get; set; } = string.Empty;
    public CompatibilitySeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string[] RelatedSlots { get; set; } = Array.Empty<string>();
}

public class CompatibilityReport
{
    public bool IsCompatible => Issues.All(i => i.Severity != CompatibilitySeverity.Error);
    public bool HasWarnings => Issues.Any(i => i.Severity == CompatibilitySeverity.Warning);
    public int? EstimatedWattage { get; set; }
    public int? RecommendedPsuWattage { get; set; }
    public int? SelectedPsuWattage { get; set; }
    public List<CompatibilityIssue> Issues { get; set; } = new();

    public string SummaryStatus(int selectedCount, int coreSelectedCount, int coreTotalCount)
    {
        if (selectedCount == 0) return "Chưa chọn linh kiện";
        if (!IsCompatible) return "Không tương thích";
        if (HasWarnings) return "Tương thích (có cảnh báo)";
        if (coreSelectedCount < coreTotalCount) return "Đang chọn cấu hình";
        return "Cấu hình tương thích tốt";
    }
}
