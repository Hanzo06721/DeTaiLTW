using Microsoft.AspNetCore.Mvc;
using PCStore.ViewModels;

namespace PCStore.Areas.Admin.Controllers;

[Area("Admin")]
public class SuppliersController : Controller
{
    public IActionResult Index() => View(new AdminModuleViewModel
    {
        Title = "Nhà cung cấp",
        Description = "Quản lý đơn vị cung cấp PC và các loại linh kiện.",
        Symbol = "▦",
        Columns = new[] { "Mã", "Tên nhà cung cấp", "Điện thoại", "Email", "Thao tác" }
    });
}
