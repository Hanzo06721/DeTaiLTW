using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using PCStore.ViewModels.Builder;

namespace PCStore.Services.Builder;

public class CompatibilityService : ICompatibilityService
{
    public CompatibilityReport Evaluate(IReadOnlyDictionary<string, BuildPcSlotRowViewModel> selectedRows)
    {
        var report = new CompatibilityReport();
        if (selectedRows.Count == 0)
        {
            return report;
        }

        selectedRows.TryGetValue("CPU", out var cpuRow);
        selectedRows.TryGetValue("MAINBOARD", out var mainRow);
        selectedRows.TryGetValue("RAM", out var ramRow);
        selectedRows.TryGetValue("VGA", out var vgaRow);
        selectedRows.TryGetValue("SSD", out var ssdRow);
        selectedRows.TryGetValue("PSU", out var psuRow);
        selectedRows.TryGetValue("CASE", out var caseRow);

        var cpu = cpuRow?.SelectedProduct;
        var main = mainRow?.SelectedProduct;
        var ram = ramRow?.SelectedProduct;
        var vga = vgaRow?.SelectedProduct;
        var psu = psuRow?.SelectedProduct;
        var pcCase = caseRow?.SelectedProduct;

        // 1. Kiểm tra tồn kho của các linh kiện đã chọn
        foreach (var row in selectedRows.Values)
        {
            if (row.SelectedProduct is null) continue;
            if (row.SelectedProduct.StockQuantity <= 0)
            {
                report.Issues.Add(new CompatibilityIssue
                {
                    Code = "OUT_OF_STOCK",
                    Severity = CompatibilitySeverity.Error,
                    Title = $"Hết hàng ({row.Slot.Title})",
                    Message = $"{row.SelectedProduct.Name} hiện đã hết hàng trong kho.",
                    RelatedSlots = new[] { row.Slot.Key }
                });
            }
            else if (row.Quantity > row.SelectedProduct.StockQuantity)
            {
                report.Issues.Add(new CompatibilityIssue
                {
                    Code = "STOCK_EXCEEDED",
                    Severity = CompatibilitySeverity.Error,
                    Title = $"Vượt tồn kho ({row.Slot.Title})",
                    Message = $"{row.SelectedProduct.Name} chỉ còn {row.SelectedProduct.StockQuantity} sản phẩm trong kho (đang chọn {row.Quantity}).",
                    RelatedSlots = new[] { row.Slot.Key }
                });
            }
        }

        // 2. Kiểm tra Socket CPU - Mainboard
        if (cpu is not null && main is not null)
        {
            var cpuSocketRaw = ExtractSocket(cpu);
            var mainSocketRaw = ExtractSocket(main);

            if (!string.IsNullOrWhiteSpace(cpuSocketRaw) && !string.IsNullOrWhiteSpace(mainSocketRaw))
            {
                var cpuSocket = NormalizeSocket(cpuSocketRaw);
                var mainSocket = NormalizeSocket(mainSocketRaw);
                if (cpuSocket == mainSocket)
                {
                    report.Issues.Add(new CompatibilityIssue
                    {
                        Code = "SOCKET_OK",
                        Severity = CompatibilitySeverity.Ok,
                        Title = "Socket CPU & Mainboard khớp",
                        Message = $"CPU ({cpuSocketRaw}) tương thích hoàn toàn với Mainboard ({mainSocketRaw}).",
                        RelatedSlots = new[] { "CPU", "MAINBOARD" }
                    });
                }
                else
                {
                    report.Issues.Add(new CompatibilityIssue
                    {
                        Code = "SOCKET_MISMATCH",
                        Severity = CompatibilitySeverity.Error,
                        Title = "Không tương thích Socket CPU – Mainboard",
                        Message = $"CPU dùng socket {cpuSocketRaw} nhưng Mainboard hỗ trợ socket {mainSocketRaw}. Hãy đổi CPU hoặc Mainboard cùng socket.",
                        RelatedSlots = new[] { "CPU", "MAINBOARD" }
                    });
                }
            }
            else
            {
                report.Issues.Add(new CompatibilityIssue
                {
                    Code = "SOCKET_DATA_MISSING",
                    Severity = CompatibilitySeverity.Info,
                    Title = "Thiếu thông số Socket",
                    Message = "Chưa đủ dữ liệu thuộc tính Socket để tự động đối chiếu giữa CPU và Mainboard.",
                    RelatedSlots = new[] { "CPU", "MAINBOARD" }
                });
            }
        }

        // 3. Kiểm tra chuẩn RAM (Mainboard - RAM, và CPU - RAM nếu CPU có thuộc tính chuẩn RAM)
        if (main is not null && ram is not null)
        {
            var mainRamTypes = ExtractRamTypes(main);
            var ramTypes = ExtractRamTypes(ram);

            if (mainRamTypes.Count > 0 && ramTypes.Count > 0)
            {
                var overlap = mainRamTypes.Intersect(ramTypes, StringComparer.OrdinalIgnoreCase).ToList();
                if (overlap.Count > 0)
                {
                    report.Issues.Add(new CompatibilityIssue
                    {
                        Code = "RAM_TYPE_OK",
                        Severity = CompatibilitySeverity.Ok,
                        Title = "Chuẩn RAM tương thích",
                        Message = $"RAM chuẩn {string.Join("/", ramTypes)} tương thích với Mainboard hỗ trợ {string.Join("/", mainRamTypes)}.",
                        RelatedSlots = new[] { "MAINBOARD", "RAM" }
                    });
                }
                else
                {
                    report.Issues.Add(new CompatibilityIssue
                    {
                        Code = "RAM_TYPE_MISMATCH",
                        Severity = CompatibilitySeverity.Error,
                        Title = "Không tương thích chuẩn RAM",
                        Message = $"Mainboard hỗ trợ chuẩn {string.Join("/", mainRamTypes)} nhưng RAM đã chọn là {string.Join("/", ramTypes)}.",
                        RelatedSlots = new[] { "MAINBOARD", "RAM" }
                    });
                }
            }
            else
            {
                report.Issues.Add(new CompatibilityIssue
                {
                    Code = "RAM_TYPE_DATA_MISSING",
                    Severity = CompatibilitySeverity.Info,
                    Title = "Thiếu thông số chuẩn RAM",
                    Message = "Chưa đủ dữ liệu chuẩn RAM (DDR4/DDR5) để kiểm tra tự động giữa Mainboard và RAM.",
                    RelatedSlots = new[] { "MAINBOARD", "RAM" }
                });
            }

            // Kiểm tra số khe RAM của Mainboard so với số lượng thanh RAM đang chọn
            var mainRamSlots = ExtractIntValue(main, "số khe ram", "so khe ram", "khe ram", "ram slots", "memory slots");
            if (mainRamSlots.HasValue && ramRow is not null && ramRow.Quantity > mainRamSlots.Value)
            {
                report.Issues.Add(new CompatibilityIssue
                {
                    Code = "RAM_SLOTS_EXCEEDED",
                    Severity = CompatibilitySeverity.Error,
                    Title = "Vượt số khe RAM của Mainboard",
                    Message = $"Mainboard chỉ có {mainRamSlots.Value} khe RAM nhưng bạn đang chọn số lượng {ramRow.Quantity}.",
                    RelatedSlots = new[] { "MAINBOARD", "RAM" }
                });
            }
        }
        else if (cpu is not null && ram is not null)
        {
            var cpuRamTypes = ExtractRamTypes(cpu);
            var ramTypes = ExtractRamTypes(ram);
            if (cpuRamTypes.Count > 0 && ramTypes.Count > 0)
            {
                var overlap = cpuRamTypes.Intersect(ramTypes, StringComparer.OrdinalIgnoreCase).ToList();
                if (overlap.Count == 0)
                {
                    report.Issues.Add(new CompatibilityIssue
                    {
                        Code = "CPU_RAM_MISMATCH",
                        Severity = CompatibilitySeverity.Error,
                        Title = "CPU không hỗ trợ chuẩn RAM đã chọn",
                        Message = $"CPU hỗ trợ {string.Join("/", cpuRamTypes)} nhưng RAM đã chọn là {string.Join("/", ramTypes)}.",
                        RelatedSlots = new[] { "CPU", "RAM" }
                    });
                }
            }
        }

        // 4. Kiểm tra Form Factor Mainboard - Case (nếu có đủ dữ liệu)
        if (main is not null && pcCase is not null)
        {
            var mainForm = ExtractFormFactors(main);
            var caseForms = ExtractFormFactors(pcCase);
            if (mainForm.Count > 0 && caseForms.Count > 0)
            {
                var fits = mainForm.Any(f => caseForms.Contains(f, StringComparer.OrdinalIgnoreCase));
                if (fits)
                {
                    report.Issues.Add(new CompatibilityIssue
                    {
                        Code = "FORM_FACTOR_OK",
                        Severity = CompatibilitySeverity.Ok,
                        Title = "Kích thước Mainboard & Case phù hợp",
                        Message = $"Case ({string.Join("/", caseForms)}) lắp vừa Mainboard chuẩn {string.Join("/", mainForm)}.",
                        RelatedSlots = new[] { "MAINBOARD", "CASE" }
                    });
                }
                else
                {
                    report.Issues.Add(new CompatibilityIssue
                    {
                        Code = "FORM_FACTOR_MISMATCH",
                        Severity = CompatibilitySeverity.Error,
                        Title = "Kích thước Mainboard không vừa Case",
                        Message = $"Mainboard chuẩn {string.Join("/", mainForm)} nhưng Case chỉ hỗ trợ {string.Join("/", caseForms)}.",
                        RelatedSlots = new[] { "MAINBOARD", "CASE" }
                    });
                }
            }
        }

        // 5. Tính công suất ước tính & kiểm tra nguồn (PSU)
        var cpuTdp = cpu is not null ? ExtractWattage(cpu, "tdp", "công suất cpu", "cong suat cpu", "công suất tiêu thụ", "cong suat tieu thu", "công suất", "cong suat") : null;
        var vgaTdp = vga is not null ? ExtractWattage(vga, "tdp", "công suất tiêu thụ", "cong suat tieu thu", "công suất vga", "cong suat vga", "công suất", "cong suat") : null;
        var vgaRecPsu = vga is not null ? ExtractWattage(vga, "nguồn khuyến nghị", "nguon khuyen nghi", "công suất nguồn khuyến nghị", "recommended psu", "psu khuyến nghị", "psu khuyen nghi") : null;
        var psuWatt = psu is not null ? ExtractWattage(psu, "công suất nguồn", "cong suat nguon", "công suất", "cong suat", "wattage", "power", "psu") : null;

        report.SelectedPsuWattage = psuWatt;

        int? estimated = null;
        if (cpuTdp.HasValue || vgaTdp.HasValue)
        {
            var ramQty = ramRow?.Quantity ?? 0;
            var ssdQty = ssdRow?.Quantity ?? 0;
            var vgaQty = vgaRow?.Quantity ?? 1;
            var baseOverhead = 75 + (ramQty * 8) + (ssdQty * 8);
            estimated = (cpuTdp ?? 65) + ((vgaTdp ?? 0) * Math.Max(1, vgaQty)) + baseOverhead;
            report.EstimatedWattage = estimated;
        }

        int? recommended = null;
        if (estimated.HasValue)
        {
            var withHeadroom = (int)Math.Ceiling(estimated.Value * 1.25 / 50.0) * 50;
            recommended = vgaRecPsu.HasValue ? Math.Max(withHeadroom, vgaRecPsu.Value) : withHeadroom;
        }
        else if (vgaRecPsu.HasValue)
        {
            recommended = vgaRecPsu.Value;
        }

        report.RecommendedPsuWattage = recommended;

        if (psu is not null)
        {
            if (psuWatt.HasValue && (estimated.HasValue || recommended.HasValue))
            {
                var minNeed = estimated ?? recommended!.Value;
                var recNeed = recommended ?? minNeed;

                if (psuWatt.Value < minNeed)
                {
                    report.Issues.Add(new CompatibilityIssue
                    {
                        Code = "PSU_INSUFFICIENT",
                        Severity = CompatibilitySeverity.Error,
                        Title = "Công suất nguồn (PSU) không đủ",
                        Message = $"Nguồn đã chọn chỉ có {psuWatt.Value}W, thấp hơn mức tiêu thụ ước tính ~{minNeed}W (khuyến nghị từ {recNeed}W trở lên).",
                        RelatedSlots = new[] { "PSU", "CPU", "VGA" }
                    });
                }
                else if (psuWatt.Value < recNeed)
                {
                    report.Issues.Add(new CompatibilityIssue
                    {
                        Code = "PSU_TIGHT",
                        Severity = CompatibilitySeverity.Warning,
                        Title = "Cảnh báo công suất nguồn sát ngưỡng",
                        Message = $"Nguồn {psuWatt.Value}W thấp hơn mức khuyến nghị {recNeed}W{(estimated.HasValue ? $" (ước tính tải ~{estimated.Value}W)" : "")}. Nên nâng lên PSU ≥ {recNeed}W để hệ thống ổn định hơn.",
                        RelatedSlots = new[] { "PSU", "CPU", "VGA" }
                    });
                }
                else
                {
                    report.Issues.Add(new CompatibilityIssue
                    {
                        Code = "PSU_OK",
                        Severity = CompatibilitySeverity.Ok,
                        Title = "Công suất nguồn (PSU) đáp ứng tốt",
                        Message = $"Nguồn {psuWatt.Value}W đủ tải cho cấu hình{(estimated.HasValue ? $" (ước tính ~{estimated.Value}W, khuyến nghị ≥ {recNeed}W)" : $" (khuyến nghị ≥ {recNeed}W)")}.",
                        RelatedSlots = new[] { "PSU" }
                    });
                }
            }
            else if (!psuWatt.HasValue)
            {
                report.Issues.Add(new CompatibilityIssue
                {
                    Code = "PSU_WATT_MISSING",
                    Severity = CompatibilitySeverity.Info,
                    Title = "Thiếu thông số công suất PSU",
                    Message = "Nguồn đã chọn chưa có thuộc tính công suất (W) nên chưa thể cảnh báo tự động.",
                    RelatedSlots = new[] { "PSU" }
                });
            }
            else if (cpu is not null || vga is not null)
            {
                report.Issues.Add(new CompatibilityIssue
                {
                    Code = "TDP_DATA_MISSING",
                    Severity = CompatibilitySeverity.Info,
                    Title = "Chưa đủ dữ liệu TDP để ước tính công suất",
                    Message = $"Đã nhận nguồn {psuWatt.Value}W, nhưng CPU/VGA chưa có thuộc tính TDP hoặc Nguồn khuyến nghị.",
                    RelatedSlots = new[] { "PSU", "CPU", "VGA" }
                });
            }
        }
        else if (recommended.HasValue)
        {
            report.Issues.Add(new CompatibilityIssue
            {
                Code = "PSU_RECOMMENDATION",
                Severity = CompatibilitySeverity.Info,
                Title = "Gợi ý chọn bộ nguồn (PSU)",
                Message = $"Với linh kiện hiện tại{(estimated.HasValue ? $" (ước tính ~{estimated.Value}W)" : "")}, nên chọn PSU công suất từ {recommended.Value}W trở lên.",
                RelatedSlots = new[] { "PSU" }
            });
        }

        return report;
    }

    public (string Status, string? Reason) EvaluateCandidate(
        string slotKey,
        BuildPcProductDto candidate,
        IReadOnlyDictionary<string, BuildPcSlotRowViewModel> currentSelectedRows)
    {
        var slot = BuildPcSlots.Find(slotKey);
        if (slot is null) return ("ok", null);

        var simulated = new Dictionary<string, BuildPcSlotRowViewModel>(currentSelectedRows, StringComparer.OrdinalIgnoreCase)
        {
            [slot.Key] = new BuildPcSlotRowViewModel
            {
                Slot = slot,
                SelectedProduct = candidate,
                Quantity = currentSelectedRows.TryGetValue(slot.Key, out var existing) && existing.SelectedProduct?.Id == candidate.Id
                    ? Math.Max(1, existing.Quantity)
                    : 1
            }
        };

        var report = Evaluate(simulated);

        var relatedError = report.Issues.FirstOrDefault(i =>
            i.Severity == CompatibilitySeverity.Error &&
            i.RelatedSlots.Contains(slot.Key, StringComparer.OrdinalIgnoreCase));
        if (relatedError is not null)
        {
            return ("error", relatedError.Message);
        }

        var relatedWarning = report.Issues.FirstOrDefault(i =>
            i.Severity == CompatibilitySeverity.Warning &&
            i.RelatedSlots.Contains(slot.Key, StringComparer.OrdinalIgnoreCase));
        if (relatedWarning is not null)
        {
            return ("warning", relatedWarning.Message);
        }

        var relatedOk = report.Issues.FirstOrDefault(i =>
            i.Severity == CompatibilitySeverity.Ok &&
            i.RelatedSlots.Contains(slot.Key, StringComparer.OrdinalIgnoreCase));
        if (relatedOk is not null)
        {
            return ("ok", relatedOk.Title);
        }

        return ("ok", null);
    }

    private static string? ExtractSocket(BuildPcProductDto product) =>
        FindAttributeAccentInsensitive(product, "socket", "cpu socket", "socket cpu");

    private static List<string> ExtractRamTypes(BuildPcProductDto product)
    {
        var raw = FindAttributeAccentInsensitive(
            product,
            "chuan ram", "loai ram", "ram type", "ho tro ram", "memory type", "ddr");
        var source = !string.IsNullOrWhiteSpace(raw) ? raw : product.Name;

        var matches = Regex.Matches(source, @"\bDDR[345]\b", RegexOptions.IgnoreCase);
        return matches
            .Select(m => m.Value.ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> ExtractFormFactors(BuildPcProductDto product)
    {
        var raw = FindAttributeAccentInsensitive(
            product,
            "form factor", "kich thuoc main", "kich thuoc", "chuan main", "ho tro mainboard", "ho tro main");
        if (string.IsNullOrWhiteSpace(raw)) return new List<string>();

        var normalized = NormalizeText(raw);
        var list = new List<string>();
        if (Regex.IsMatch(normalized, @"\be-?atx\b")) list.Add("E-ATX");
        if (Regex.IsMatch(normalized, @"\b(m-?atx|micro-?atx)\b")) list.Add("mATX");
        if (Regex.IsMatch(normalized, @"\b(mini-?itx|itx)\b")) list.Add("Mini-ITX");
        // Khớp ATX độc lập (không phải mATX / E-ATX / Micro-ATX)
        if (Regex.IsMatch(normalized, @"(?<![a-z-])atx\b")) list.Add("ATX");
        return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static int? ExtractWattage(BuildPcProductDto product, params string[] keys)
    {
        var raw = FindAttributeAccentInsensitive(product, keys);
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var match = Regex.Match(raw, @"(\d{2,4})\s*w?\b", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var watts) && watts >= 15 && watts <= 3000)
        {
            return watts;
        }

        return null;
    }

    private static int? ExtractIntValue(BuildPcProductDto product, params string[] keys)
    {
        var raw = FindAttributeAccentInsensitive(product, keys);
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var match = Regex.Match(raw, @"\d+");
        return match.Success && int.TryParse(match.Value, out var number) ? number : null;
    }

    private static string? FindAttributeAccentInsensitive(BuildPcProductDto product, params string[] keys)
    {
        var normalizedKeys = keys.Select(NormalizeText).ToArray();

        foreach (var nk in normalizedKeys)
        {
            var exact = product.Attributes.FirstOrDefault(a => NormalizeText(a.Name) == nk);
            if (exact is not null && !string.IsNullOrWhiteSpace(exact.Value))
                return exact.Value.Trim();
        }

        foreach (var nk in normalizedKeys)
        {
            var partial = product.Attributes.FirstOrDefault(a => NormalizeText(a.Name).Contains(nk, StringComparison.Ordinal));
            if (partial is not null && !string.IsNullOrWhiteSpace(partial.Value))
                return partial.Value.Trim();
        }

        return null;
    }

    private static string NormalizeSocket(string raw)
    {
        var cleaned = NormalizeText(raw)
            .Replace("socket", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal)
            .Replace("-", "", StringComparison.Ordinal);
        return cleaned.ToUpperInvariant();
    }

    private static string NormalizeText(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var formD = input.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }

        return Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC), @"\s+", " ").Trim();
    }
}
