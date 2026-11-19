using Microsoft.AspNetCore.Mvc;
using PCStore.ViewModels;

namespace PCStore.Areas.Admin.Controllers;

[Area("Admin")]
public class ImportsController : Controller
{
    public IActionResult Index() => View(new AdminModuleViewModel
    {
        Title = "Nhập hàng & tồn kho",
        Description = "Tạo phiếu nhập, cộng số lượng tồn và theo dõi nhập hàng.",
        Symbol = "↧",
        Columns = new[] { "Mã phiếu", "Nhà cung cấp", "Ngày nhập", "Tổng tiền", "Thao tác" }
    });
}
