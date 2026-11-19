using Microsoft.AspNetCore.Mvc;

namespace PCStore.Controllers;

// Chỉ điều hướng giao diện; nghiệp vụ / SQL sẽ được các thành viên thêm sau.
public class CheckoutController : Controller
{
    public IActionResult Index() => View();
}
