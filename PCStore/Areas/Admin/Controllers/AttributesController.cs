using Microsoft.AspNetCore.Mvc;
using PCStore.ViewModels;

namespace PCStore.Areas.Admin.Controllers;

[Area("Admin")]
public class AttributesController : Controller
{
    public IActionResult Index() => View(new AdminModuleViewModel
    {
        Title = "Thông số kỹ thuật",
        Description = "Các thuộc tính hỗ trợ bộ lọc và kiểm tra tương thích Build PC.",
        Symbol = "≡",
        Columns = new[] { "Mã", "Tên thuộc tính", "Giá trị mẫu", "Thao tác" }
    });
}
