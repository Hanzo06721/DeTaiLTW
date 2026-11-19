using Microsoft.AspNetCore.Mvc;
using PCStore.ViewModels;

namespace PCStore.Areas.Admin.Controllers;

[Area("Admin")]
public class ReportsController : Controller
{
    public IActionResult Index() => View(new AdminModuleViewModel
    {
        Title = "Thống kê & báo cáo",
        Description = "Tổng hợp số đơn hàng, doanh thu và sản phẩm bán chạy.",
        Symbol = "▥",
        Columns = new[] { "Chỉ số", "Kỳ báo cáo", "Giá trị", "So sánh" }
    });
}
