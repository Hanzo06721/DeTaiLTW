using Microsoft.AspNetCore.Mvc;
using PCStore.Services.Builder;

namespace PCStore.Controllers;

public class BuildPcController : Controller
{
    private readonly IBuildPcService _buildPcService;
    private readonly IBuildPcExportService _exportService;

    public BuildPcController(
        IBuildPcService buildPcService,
        IBuildPcExportService exportService)
    {
        _buildPcService = buildPcService;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _buildPcService.GetBuildPageAsync(HttpContext.Session, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Products(string slot, string? q, CancellationToken cancellationToken)
    {
        var items = await _buildPcService.SearchSlotProductsAsync(HttpContext.Session, slot, q, cancellationToken);
        return Json(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Select(string slotKey, long productId, int quantity = 1, CancellationToken cancellationToken = default)
    {
        var result = await _buildPcService.SelectComponentAsync(HttpContext.Session, slotKey, productId, quantity, cancellationToken);
        return await RespondAfterMutationAsync(result, cancellationToken);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(string slotKey, int quantity, CancellationToken cancellationToken = default)
    {
        var result = await _buildPcService.UpdateQuantityAsync(HttpContext.Session, slotKey, quantity, cancellationToken);
        return await RespondAfterMutationAsync(result, cancellationToken);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string slotKey, CancellationToken cancellationToken = default)
    {
        var result = _buildPcService.RemoveComponent(HttpContext.Session, slotKey);
        return await RespondAfterMutationAsync(result, cancellationToken);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken = default)
    {
        var result = _buildPcService.ClearBuild(HttpContext.Session);
        return await RespondAfterMutationAsync(result, cancellationToken);
    }

    private async Task<IActionResult> RespondAfterMutationAsync(
        BuildPcOperationResult result,
        CancellationToken cancellationToken)
    {
        if (string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
        {
            var model = await _buildPcService.GetBuildPageAsync(HttpContext.Session, cancellationToken);
            ViewData[result.Success ? "BuildPcSuccess" : "BuildPcError"] = result.Message;
            return PartialView("_BuildPcWorkspace", model);
        }

        TempData[result.Success ? "BuildPcSuccess" : "BuildPcError"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(CancellationToken cancellationToken)
    {
        var model = await _buildPcService.GetBuildPageAsync(HttpContext.Session, cancellationToken);
        if (!model.HasItems)
        {
            TempData["BuildPcError"] = "Vui lòng chọn ít nhất 1 linh kiện trước khi xuất báo giá PDF.";
            return RedirectToAction(nameof(Index));
        }

        var bytes = _exportService.ExportPdf(model);
        var fileName = $"BaoGia_PCStore_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(CancellationToken cancellationToken)
    {
        var model = await _buildPcService.GetBuildPageAsync(HttpContext.Session, cancellationToken);
        if (!model.HasItems)
        {
            TempData["BuildPcError"] = "Vui lòng chọn ít nhất 1 linh kiện trước khi xuất báo giá Excel.";
            return RedirectToAction(nameof(Index));
        }

        var bytes = _exportService.ExportExcel(model);
        var fileName = $"BaoGia_PCStore_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(CancellationToken cancellationToken)
    {
        var result = await _buildPcService.TransferToCartAsync(HttpContext.Session, cancellationToken);
        if (!result.Success)
        {
            TempData["BuildPcError"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["CartNotice"] = result.Message;
        return RedirectToAction("Index", "Cart");
    }
}
