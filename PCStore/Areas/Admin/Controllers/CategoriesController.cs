using Microsoft.AspNetCore.Mvc;
using PCStore.ViewModels;

namespace PCStore.Areas.Admin.Controllers;

[Area("Admin")]
public class CategoriesController : Controller
{
    public IActionResult Index() => View(new AdminModuleViewModel
    {
        Title = "Danh mục sản phẩm",
        Description = "Cấu trúc danh mục đa cấp: PC, CPU, VGA, Mainboard, RAM...",
        Symbol = "◫",
        Columns = new[] { "Mã", "Tên danh mục", "Danh mục cha", "Thao tác" }
    });
}
