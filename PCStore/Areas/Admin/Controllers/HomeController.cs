using Microsoft.AspNetCore.Mvc;
namespace PCStore.Areas.Admin.Controllers;
[Area("Admin")]
public class HomeController : Controller
{
    public IActionResult Index() => View();
}
