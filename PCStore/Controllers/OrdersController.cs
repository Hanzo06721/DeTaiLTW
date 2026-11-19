using Microsoft.AspNetCore.Mvc;

namespace PCStore.Controllers;

// Chỉ điều hướng giao diện; nghiệp vụ / SQL sẽ được các thành viên thêm sau.
public class OrdersController : Controller
{
    public IActionResult Index() => RedirectToAction(nameof(History));
    public IActionResult History() => View();
}
