namespace PCStore.ViewModels.Builder;

public sealed class BuildPcSlotDefinition
{
    public string Key { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string Icon { get; init; } = "◈";
    public bool IsCore { get; init; }
    public bool AllowMultipleQuantity { get; init; }
    public int MaxQuantity { get; init; } = 1;
    public string[] CategoryKeywords { get; init; } = Array.Empty<string>();
}

public static class BuildPcSlots
{
    public static readonly IReadOnlyList<BuildPcSlotDefinition> All = new List<BuildPcSlotDefinition>
    {
        new()
        {
            Key = "CPU",
            Number = "01",
            Title = "CPU",
            Subtitle = "Bộ vi xử lý",
            Icon = "◈",
            IsCore = true,
            AllowMultipleQuantity = false,
            MaxQuantity = 1,
            CategoryKeywords = new[] { "cpu", "vi xu ly", "vi xử lý", "processor" }
        },
        new()
        {
            Key = "MAINBOARD",
            Number = "02",
            Title = "Mainboard",
            Subtitle = "Bo mạch chủ",
            Icon = "◫",
            IsCore = true,
            AllowMultipleQuantity = false,
            MaxQuantity = 1,
            CategoryKeywords = new[] { "mainboard", "main", "bo mach chu", "bo mạch chủ", "motherboard" }
        },
        new()
        {
            Key = "RAM",
            Number = "03",
            Title = "RAM",
            Subtitle = "Bộ nhớ trong",
            Icon = "≡",
            IsCore = true,
            AllowMultipleQuantity = true,
            MaxQuantity = 8,
            CategoryKeywords = new[] { "ram", "bo nho", "bộ nhớ", "memory" }
        },
        new()
        {
            Key = "VGA",
            Number = "04",
            Title = "VGA",
            Subtitle = "Card đồ họa",
            Icon = "▥",
            IsCore = true,
            AllowMultipleQuantity = false,
            MaxQuantity = 2,
            CategoryKeywords = new[] { "vga", "gpu", "card do hoa", "card đồ họa", "card man hinh", "card màn hình" }
        },
        new()
        {
            Key = "SSD",
            Number = "05",
            Title = "SSD / HDD",
            Subtitle = "Ổ cứng lưu trữ",
            Icon = "▤",
            IsCore = true,
            AllowMultipleQuantity = true,
            MaxQuantity = 6,
            CategoryKeywords = new[] { "ssd", "hdd", "o cung", "ổ cứng", "luu tru", "lưu trữ", "storage", "nvme" }
        },
        new()
        {
            Key = "PSU",
            Number = "06",
            Title = "PSU",
            Subtitle = "Bộ nguồn máy tính",
            Icon = "⚡",
            IsCore = true,
            AllowMultipleQuantity = false,
            MaxQuantity = 1,
            CategoryKeywords = new[] { "psu", "nguon", "nguồn", "power supply" }
        },
        new()
        {
            Key = "CASE",
            Number = "07",
            Title = "Case",
            Subtitle = "Vỏ máy tính",
            Icon = "▣",
            IsCore = false,
            AllowMultipleQuantity = false,
            MaxQuantity = 1,
            CategoryKeywords = new[] { "case", "vo may", "vỏ máy", "vo case", "vỏ case", "thung may", "thùng máy" }
        },
        new()
        {
            Key = "COOLER",
            Number = "08",
            Title = "Tản nhiệt",
            Subtitle = "Hệ thống làm mát",
            Icon = "✳",
            IsCore = false,
            AllowMultipleQuantity = true,
            MaxQuantity = 6,
            CategoryKeywords = new[] { "tan nhiet", "tản nhiệt", "cooler", "cooling", "fan", "quat", "quạt", "aio" }
        }
    };

    private static readonly Dictionary<string, BuildPcSlotDefinition> ByKey =
        All.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);

    public static BuildPcSlotDefinition? Find(string? key) =>
        string.IsNullOrWhiteSpace(key) ? null : ByKey.GetValueOrDefault(key.Trim());
}
