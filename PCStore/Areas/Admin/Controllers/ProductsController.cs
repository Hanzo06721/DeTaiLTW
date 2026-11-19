using Microsoft.AspNetCore.Mvc;
using PCStore.ViewModels;

namespace PCStore.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductsController : Controller
{
    public IActionResult Index() => View(new AdminModuleViewModel
    {
        Title = "Sản phẩm & linh kiện",
        Description = "Quản lý PC ráp sẵn và linh kiện theo từng danh mục.",
        Symbol = "▣",
        Columns = new[] { "Mã SKU", "Tên sản phẩm", "Danh mục", "Giá bán", "Tồn kho", "Thao tác" }
    });
}
