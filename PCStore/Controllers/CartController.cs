using Microsoft.AspNetCore.Mvc;
using PCStore.Services.Sales;

namespace PCStore.Controllers;

// Giỏ hàng đọc dữ liệu Session dùng chung với NV3 (Build PC); NV4 sẽ mở rộng thêm cập nhật/xóa và đặt hàng.
public class CartController : Controller
{
    public IActionResult Index()
    {
        var items = CartSessionHelper.GetCart(HttpContext.Session);
        return View(items);
    }
}
