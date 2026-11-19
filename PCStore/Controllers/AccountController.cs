using Microsoft.AspNetCore.Mvc;

namespace PCStore.Controllers;

// Chỉ điều hướng giao diện; nghiệp vụ / SQL sẽ được các thành viên thêm sau.
public class AccountController : Controller
{
    public IActionResult Login() => View();
    public IActionResult Register() => View();
    public IActionResult Profile() => View();
}
