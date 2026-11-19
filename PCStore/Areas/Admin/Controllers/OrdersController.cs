using Microsoft.AspNetCore.Mvc;
using PCStore.ViewModels;

namespace PCStore.Areas.Admin.Controllers;

[Area("Admin")]
public class OrdersController : Controller
{
    public IActionResult Index() => View(new AdminModuleViewModel
    {
        Title = "Đơn hàng",
        Description = "Xác nhận, theo dõi và cập nhật trạng thái đơn hàng.",
        Symbol = "▤",
        Columns = new[] { "Mã đơn", "Khách hàng", "Ngày đặt", "Tổng tiền", "Trạng thái", "Thao tác" }
    });
}
