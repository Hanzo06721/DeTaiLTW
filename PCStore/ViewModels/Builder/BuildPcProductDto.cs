namespace PCStore.ViewModels.Builder;

public class BuildPcAttributeItem
{
    public int AttributeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class BuildPcProductDto
{
    public long Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Thumbnail { get; set; }
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<BuildPcAttributeItem> Attributes { get; set; } = new();

    public string? FindAttributeValue(params string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            var exact = Attributes.FirstOrDefault(a =>
                string.Equals(a.Name.Trim(), keyword, StringComparison.OrdinalIgnoreCase));
            if (exact is not null && !string.IsNullOrWhiteSpace(exact.Value))
                return exact.Value.Trim();
        }

        foreach (var keyword in keywords)
        {
            var partial = Attributes.FirstOrDefault(a =>
                a.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            if (partial is not null && !string.IsNullOrWhiteSpace(partial.Value))
                return partial.Value.Trim();
        }

        return null;
    }
}

public class BuildPcCandidateDto
{
    public BuildPcProductDto Product { get; set; } = new();
    public bool IsSelected { get; set; }
    public string CompatibilityStatus { get; set; } = "ok"; // ok | warning | error
    public string? CompatibilityReason { get; set; }
}
