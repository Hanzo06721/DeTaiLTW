using Microsoft.AspNetCore.Mvc;
using PCStore.ViewModels;

namespace PCStore.Areas.Admin.Controllers;

[Area("Admin")]
public class UsersController : Controller
{
    public IActionResult Index() => View(new AdminModuleViewModel
    {
        Title = "Tài khoản & phân quyền",
        Description = "Theo dõi tài khoản, trạng thái và quyền truy cập hệ thống.",
        Symbol = "♙",
        Columns = new[] { "Mã", "Họ tên", "Tên tài khoản", "Email", "Trạng thái", "Thao tác" }
    });
}
