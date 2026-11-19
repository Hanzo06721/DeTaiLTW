using System.Globalization;
using ClosedXML.Excel;
using PCStore.ViewModels.Builder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PCStore.Services.Builder;

public class BuildPcExportService : IBuildPcExportService
{
    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

    static BuildPcExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseSystemFonts = true;
        QuestPDF.Settings.ThrowOnMissingFontFamilies = false;
    }

    public byte[] ExportPdf(BuildPcPageViewModel model)
    {
        var selectedRows = model.Rows.Where(r => r.SelectedProduct is not null).ToList();
        var createdAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        var statusText = model.Compatibility.SummaryStatus(model.SelectedCount, model.CoreSelectedCount, model.CoreTotalSlots);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9.5f).FontColor("#112F49"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("PCSTORE").FontSize(20).Bold().FontColor("#02A4EF");
                            left.Item().Text("BẢNG BÁO GIÁ CẤU HÌNH PC TỰ CHỌN").FontSize(12).Bold().FontColor("#082F4D");
                        });

                        row.ConstantItem(175).AlignRight().Column(right =>
                        {
                            right.Item().Text($"Ngày lập: {createdAt}").FontSize(8.5f).FontColor("#607B91");
                            right.Item().Text($"Trạng thái: {statusText}").FontSize(8.5f).SemiBold()
                                .FontColor(model.Compatibility.IsCompatible ? "#015F8A" : "#C62828");
                            if (model.Compatibility.EstimatedWattage.HasValue)
                            {
                                right.Item().Text($"Ước tính công suất: ~{model.Compatibility.EstimatedWattage}W").FontSize(8.5f).FontColor("#607B91");
                            }
                        });
                    });

                    col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor("#02A4EF");
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(12);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(28);   // STT
                            columns.ConstantColumn(68);   // Nhóm
                            columns.RelativeColumn(3.2f); // Tên + SKU + Thông số
                            columns.ConstantColumn(32);   // SL
                            columns.ConstantColumn(82);   // Đơn giá
                            columns.ConstantColumn(90);   // Thành tiền
                        });

                        table.Header(header =>
                        {
                            static IContainer HeaderCell(IContainer c) => c
                                .Background("#015F8A")
                                .PaddingVertical(6)
                                .PaddingHorizontal(6)
                                .DefaultTextStyle(t => t.FontColor(Colors.White).SemiBold().FontSize(9));

                            header.Cell().Element(HeaderCell).AlignCenter().Text("#");
                            header.Cell().Element(HeaderCell).Text("Nhóm");
                            header.Cell().Element(HeaderCell).Text("Linh kiện & Thông số chính");
                            header.Cell().Element(HeaderCell).AlignCenter().Text("SL");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Đơn giá");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Thành tiền");
                        });

                        var index = 1;
                        foreach (var row in selectedRows)
                        {
                            var product = row.SelectedProduct!;
                            var bg = index % 2 == 0 ? "#F6F9FD" : "#FFFFFF";
                            var specsText = product.Attributes.Count > 0
                                ? string.Join(" · ", product.Attributes.Take(4).Select(a => $"{a.Name}: {a.Value}"))
                                : "Đang cập nhật thông số";

                            IContainer DataCell(IContainer c) => c
                                .Background(bg)
                                .BorderBottom(0.5f)
                                .BorderColor("#E5EDF5")
                                .PaddingVertical(6)
                                .PaddingHorizontal(6);

                            table.Cell().Element(DataCell).AlignCenter().Text(index.ToString());
                            table.Cell().Element(DataCell).Text(row.Slot.Title).SemiBold();
                            table.Cell().Element(DataCell).Column(p =>
                            {
                                p.Item().Text(product.Name).SemiBold();
                                p.Item().Text($"SKU: {product.Sku} | {specsText}").FontSize(8).FontColor("#607B91");
                            });
                            table.Cell().Element(DataCell).AlignCenter().Text(row.Quantity.ToString());
                            table.Cell().Element(DataCell).AlignRight().Text(FormatVnd(product.Price));
                            table.Cell().Element(DataCell).AlignRight().Text(FormatVnd(row.LineTotal)).SemiBold();
                            index++;
                        }
                    });

                    col.Item().AlignRight().Background("#EAF8FF").Border(1).BorderColor("#B8E3FA").Padding(10).Row(r =>
                    {
                        r.AutoItem().PaddingRight(16).Text($"TỔNG CỘNG ({model.TotalItemsQuantity} sản phẩm):").Bold().FontSize(11).FontColor("#015F8A");
                        r.AutoItem().Text(FormatVnd(model.TotalPrice)).Bold().FontSize(13).FontColor("#EF4D02");
                    });

                    if (model.Compatibility.Issues.Count > 0)
                    {
                        col.Item().Column(notes =>
                        {
                            notes.Spacing(4);
                            notes.Item().Text("KẾT QUẢ KIỂM TRA TƯƠNG THÍCH").Bold().FontSize(10).FontColor("#015F8A");
                            foreach (var issue in model.Compatibility.Issues)
                            {
                                var (prefix, color) = issue.Severity switch
                                {
                                    CompatibilitySeverity.Error => ("[LỖI]", "#C62828"),
                                    CompatibilitySeverity.Warning => ("[CẢNH BÁO]", "#B26A00"),
                                    CompatibilitySeverity.Ok => ("[OK]", "#0E7A56"),
                                    _ => ("[THÔNG TIN]", "#3D6782")
                                };

                                notes.Item().Text(text =>
                                {
                                    text.Span($"{prefix} ").Bold().FontColor(color);
                                    text.Span(issue.Message).FontSize(8.8f);
                                });
                            }
                        });
                    }
                });

                page.Footer().Column(f =>
                {
                    f.Item().LineHorizontal(0.5f).LineColor("#D8E6F0");
                    f.Item().PaddingTop(6).Row(r =>
                    {
                        r.RelativeItem().Text("Giá mang tính tham khảo tại thời điểm xuất báo giá · PCStore").FontSize(8).FontColor("#7B91A3");
                        r.ConstantItem(80).AlignRight().Text(x =>
                        {
                            x.DefaultTextStyle(t => t.FontSize(8).FontColor("#7B91A3"));
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                });
            });
        }).GeneratePdf();
    }

    public byte[] ExportExcel(BuildPcPageViewModel model)
    {
        var selectedRows = model.Rows.Where(r => r.SelectedProduct is not null).ToList();
        var statusText = model.Compatibility.SummaryStatus(model.SelectedCount, model.CoreSelectedCount, model.CoreTotalSlots);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("BaoGiaPC");

        var headers = new[] { "STT", "Nhóm linh kiện", "Mã SKU", "Tên sản phẩm", "Thông số kỹ thuật", "SL", "Đơn giá (VNĐ)", "Thành tiền (VNĐ)" };

        ws.Cell(1, 1).Value = "PCSTORE - BẢNG BÁO GIÁ CẤU HÌNH PC TỰ CHỌN";
        ws.Range(1, 1, 1, headers.Length).Merge().Style
            .Font.SetBold()
            .Font.SetFontSize(14)
            .Font.SetFontColor(XLColor.FromHtml("#015F8A"))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(2, 1).Value = $"Ngày lập: {DateTime.Now:dd/MM/yyyy HH:mm} | Trạng thái: {statusText}";
        ws.Range(2, 1, 2, headers.Length).Merge().Style
            .Font.SetFontSize(10)
            .Font.SetFontColor(XLColor.FromHtml("#607B91"))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        var headerRow = 4;

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.SetBold();
            cell.Style.Font.SetFontColor(XLColor.White);
            cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#015F8A"));
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        var currentRow = headerRow + 1;
        var stt = 1;
        foreach (var row in selectedRows)
        {
            var product = row.SelectedProduct!;
            var specs = product.Attributes.Count > 0
                ? string.Join("; ", product.Attributes.Select(a => $"{a.Name}: {a.Value}"))
                : "";

            ws.Cell(currentRow, 1).Value = stt;
            ws.Cell(currentRow, 2).Value = row.Slot.Title;
            ws.Cell(currentRow, 3).Value = product.Sku;
            ws.Cell(currentRow, 4).Value = product.Name;
            ws.Cell(currentRow, 5).Value = specs;
            ws.Cell(currentRow, 6).Value = row.Quantity;
            ws.Cell(currentRow, 7).Value = product.Price;
            ws.Cell(currentRow, 8).Value = row.LineTotal;

            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";
            ws.Cell(currentRow, 8).Style.NumberFormat.Format = "#,##0";

            var rowRange = ws.Range(currentRow, 1, currentRow, headers.Length);
            rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            if (stt % 2 == 0)
            {
                rowRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F6F9FD");
            }

            stt++;
            currentRow++;
        }

        // Dòng tổng cộng
        ws.Cell(currentRow, 1).Value = "TỔNG CỘNG";
        ws.Range(currentRow, 1, currentRow, 5).Merge().Style
            .Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        ws.Cell(currentRow, 6).Value = model.TotalItemsQuantity;
        ws.Cell(currentRow, 6).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell(currentRow, 8).Value = model.TotalPrice;
        ws.Cell(currentRow, 8).Style.Font.SetBold().Font.SetFontColor(XLColor.FromHtml("#EF4D02")).NumberFormat.Format = "#,##0 \"đ\"";
        ws.Range(currentRow, 1, currentRow, headers.Length).Style
            .Fill.SetBackgroundColor(XLColor.FromHtml("#EAF8FF"))
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        // Ghi chú kiểm tra tương thích
        if (model.Compatibility.Issues.Count > 0)
        {
            currentRow += 2;
            ws.Cell(currentRow, 1).Value = "KẾT QUẢ KIỂM TRA TƯƠNG THÍCH";
            ws.Range(currentRow, 1, currentRow, headers.Length).Merge().Style.Font.SetBold().Font.SetFontColor(XLColor.FromHtml("#015F8A"));
            currentRow++;

            foreach (var issue in model.Compatibility.Issues)
            {
                var label = issue.Severity switch
                {
                    CompatibilitySeverity.Error => "LỖI",
                    CompatibilitySeverity.Warning => "CẢNH BÁO",
                    CompatibilitySeverity.Ok => "TƯƠNG THÍCH",
                    _ => "THÔNG TIN"
                };

                ws.Cell(currentRow, 1).Value = label;
                ws.Cell(currentRow, 2).Value = issue.Message;
                ws.Range(currentRow, 2, currentRow, headers.Length).Merge();
                currentRow++;
            }
        }

        ws.Columns().AdjustToContents();
        ws.Column(4).Width = Math.Min(Math.Max(ws.Column(4).Width, 28), 46);
        ws.Column(5).Width = Math.Min(Math.Max(ws.Column(5).Width, 30), 50);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static string FormatVnd(decimal value) => $"{value.ToString("#,##0", ViCulture)} đ";
}
