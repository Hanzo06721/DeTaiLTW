using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PCStore.Repositories.Builder;
using PCStore.Services.Sales;
using PCStore.ViewModels.Builder;
using PCStore.ViewModels.Sales;

namespace PCStore.Services.Builder;

public class BuildPcService : IBuildPcService
{
    public const string BuildSessionKey = "PCStore.BuildPc";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IBuildPcRepository _repository;
    private readonly ICompatibilityService _compatibilityService;

    public BuildPcService(
        IBuildPcRepository repository,
        ICompatibilityService compatibilityService)
    {
        _repository = repository;
        _compatibilityService = compatibilityService;
    }

    public async Task<BuildPcPageViewModel> GetBuildPageAsync(
        ISession session,
        CancellationToken cancellationToken = default)
    {
        var savedItems = LoadSelection(session);
        var productIds = savedItems.Values.Select(i => i.ProductId).Distinct().ToList();

        var productsMap = await _repository.GetProductsByIdsAsync(productIds, cancellationToken);
        var countsMap = await _repository.GetAvailableCountsBySlotAsync(cancellationToken);

        var rows = new List<BuildPcSlotRowViewModel>();
        var selectedRowsDict = new Dictionary<string, BuildPcSlotRowViewModel>(StringComparer.OrdinalIgnoreCase);

        foreach (var slot in BuildPcSlots.All)
        {
            var row = new BuildPcSlotRowViewModel
            {
                Slot = slot,
                AvailableProductsCount = countsMap.GetValueOrDefault(slot.Key)
            };

            if (savedItems.TryGetValue(slot.Key, out var saved) &&
                productsMap.TryGetValue(saved.ProductId, out var product))
            {
                row.SelectedProduct = product;
                row.Quantity = Math.Max(1, saved.Quantity);

                if (product.StockQuantity <= 0)
                {
                    row.StockNote = "Sản phẩm đã hết hàng";
                }
                else if (row.Quantity > product.StockQuantity)
                {
                    row.StockNote = $"Chỉ còn {product.StockQuantity} trong kho";
                }

                selectedRowsDict[slot.Key] = row;
            }

            rows.Add(row);
        }

        var report = _compatibilityService.Evaluate(selectedRowsDict);

        return new BuildPcPageViewModel
        {
            Rows = rows,
            Compatibility = report,
            CartCount = CartSessionHelper.GetTotalQuantity(session)
        };
    }

    public async Task<List<BuildPcCandidateDto>> SearchSlotProductsAsync(
        ISession session,
        string slotKey,
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var slot = BuildPcSlots.Find(slotKey);
        if (slot is null)
        {
            return new List<BuildPcCandidateDto>();
        }

        var candidates = await _repository.GetAvailableProductsBySlotAsync(slot, keyword, cancellationToken);
        var savedItems = LoadSelection(session);
        var currentSelectedId = savedItems.TryGetValue(slot.Key, out var currentItem)
            ? currentItem.ProductId
            : (long?)null;

        var selectedIds = savedItems.Values.Select(i => i.ProductId).Distinct().ToList();
        var productsMap = await _repository.GetProductsByIdsAsync(selectedIds, cancellationToken);

        var currentRows = new Dictionary<string, BuildPcSlotRowViewModel>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in savedItems)
        {
            var s = BuildPcSlots.Find(kvp.Key);
            if (s is not null && productsMap.TryGetValue(kvp.Value.ProductId, out var prod))
            {
                currentRows[s.Key] = new BuildPcSlotRowViewModel
                {
                    Slot = s,
                    SelectedProduct = prod,
                    Quantity = Math.Max(1, kvp.Value.Quantity)
                };
            }
        }

        return candidates.Select(product =>
        {
            var (status, reason) = _compatibilityService.EvaluateCandidate(slot.Key, product, currentRows);
            return new BuildPcCandidateDto
            {
                Product = product,
                IsSelected = currentSelectedId == product.Id,
                CompatibilityStatus = status,
                CompatibilityReason = reason
            };
        }).ToList();
    }

    public async Task<BuildPcOperationResult> SelectComponentAsync(
        ISession session,
        string slotKey,
        long productId,
        int quantity = 1,
        CancellationToken cancellationToken = default)
    {
        var slot = BuildPcSlots.Find(slotKey);
        if (slot is null)
        {
            return new BuildPcOperationResult(false, "Nhóm linh kiện không hợp lệ.");
        }

        var product = await _repository.GetProductByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return new BuildPcOperationResult(false, "Không tìm thấy sản phẩm trong cơ sở dữ liệu.");
        }

        if (product.StockQuantity <= 0)
        {
            return new BuildPcOperationResult(false, $"Sản phẩm \"{product.Name}\" hiện đã hết hàng.");
        }

        var maxAllowed = Math.Min(product.StockQuantity, slot.MaxQuantity);
        var normalizedQty = Math.Clamp(quantity, 1, Math.Max(1, maxAllowed));

        var saved = LoadSelection(session);
        saved[slot.Key] = new BuildPcSelectedItem
        {
            SlotKey = slot.Key,
            ProductId = product.Id,
            Quantity = normalizedQty
        };
        SaveSelection(session, saved);

        return new BuildPcOperationResult(true, $"Đã chọn {product.Name} cho nhóm {slot.Title}.");
    }

    public async Task<BuildPcOperationResult> UpdateQuantityAsync(
        ISession session,
        string slotKey,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var slot = BuildPcSlots.Find(slotKey);
        if (slot is null)
        {
            return new BuildPcOperationResult(false, "Nhóm linh kiện không hợp lệ.");
        }

        var saved = LoadSelection(session);
        if (!saved.TryGetValue(slot.Key, out var item))
        {
            return new BuildPcOperationResult(false, $"Chưa chọn linh kiện cho nhóm {slot.Title}.");
        }

        if (quantity <= 0)
        {
            saved.Remove(slot.Key);
            SaveSelection(session, saved);
            return new BuildPcOperationResult(true, $"Đã xóa linh kiện khỏi nhóm {slot.Title}.");
        }

        var product = await _repository.GetProductByIdAsync(item.ProductId, cancellationToken);
        if (product is null)
        {
            saved.Remove(slot.Key);
            SaveSelection(session, saved);
            return new BuildPcOperationResult(false, "Sản phẩm không còn tồn tại trong hệ thống.");
        }

        if (product.StockQuantity <= 0)
        {
            return new BuildPcOperationResult(false, $"Sản phẩm \"{product.Name}\" đã hết hàng.");
        }

        var maxAllowed = Math.Min(product.StockQuantity, slot.MaxQuantity);
        if (quantity > maxAllowed)
        {
            item.Quantity = maxAllowed;
            SaveSelection(session, saved);
            return new BuildPcOperationResult(
                true,
                $"Số lượng tối đa cho {slot.Title} hiện là {maxAllowed} (tồn kho: {product.StockQuantity}).");
        }

        item.Quantity = quantity;
        SaveSelection(session, saved);
        return new BuildPcOperationResult(true, $"Đã cập nhật số lượng {slot.Title} thành {quantity}.");
    }

    public BuildPcOperationResult RemoveComponent(ISession session, string slotKey)
    {
        var slot = BuildPcSlots.Find(slotKey);
        if (slot is null)
        {
            return new BuildPcOperationResult(false, "Nhóm linh kiện không hợp lệ.");
        }

        var saved = LoadSelection(session);
        if (saved.Remove(slot.Key))
        {
            SaveSelection(session, saved);
            return new BuildPcOperationResult(true, $"Đã bỏ chọn linh kiện nhóm {slot.Title}.");
        }

        return new BuildPcOperationResult(false, $"Nhóm {slot.Title} hiện chưa có linh kiện.");
    }

    public BuildPcOperationResult ClearBuild(ISession session)
    {
        session.Remove(BuildSessionKey);
        return new BuildPcOperationResult(true, "Đã xóa toàn bộ cấu hình Build PC.");
    }

    public async Task<BuildPcOperationResult> TransferToCartAsync(
        ISession session,
        CancellationToken cancellationToken = default)
    {
        var model = await GetBuildPageAsync(session, cancellationToken);
        var selectedRows = model.Rows.Where(r => r.SelectedProduct is not null).ToList();

        if (selectedRows.Count == 0)
        {
            return new BuildPcOperationResult(false, "Bạn chưa chọn linh kiện nào để thêm vào giỏ hàng.");
        }

        var stockErrors = selectedRows
            .Where(r => r.SelectedProduct!.StockQuantity <= 0 || r.Quantity > r.SelectedProduct.StockQuantity)
            .ToList();
        if (stockErrors.Count > 0)
        {
            var names = string.Join(", ", stockErrors.Select(r => r.SelectedProduct!.Name));
            return new BuildPcOperationResult(false, $"Không đủ tồn kho cho: {names}. Vui lòng điều chỉnh trước khi thêm vào giỏ.");
        }

        var cartItems = selectedRows.Select(r => new CartSessionItem
        {
            ProductId = r.SelectedProduct!.Id,
            Sku = r.SelectedProduct.Sku,
            Name = r.SelectedProduct.Name,
            Thumbnail = r.SelectedProduct.Thumbnail,
            UnitPrice = r.SelectedProduct.Price,
            Quantity = r.Quantity,
            StockQuantity = r.SelectedProduct.StockQuantity,
            Source = "BUILD_PC"
        }).ToList();

        CartSessionHelper.MergeItems(session, cartItems);
        var totalAddedQty = cartItems.Sum(i => i.Quantity);

        return new BuildPcOperationResult(
            true,
            $"Đã chuyển {selectedRows.Count} nhóm linh kiện ({totalAddedQty} sản phẩm) từ cấu hình Build PC sang giỏ hàng.",
            totalAddedQty);
    }

    private static Dictionary<string, BuildPcSelectedItem> LoadSelection(ISession session)
    {
        var raw = session.GetString(BuildSessionKey);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new Dictionary<string, BuildPcSelectedItem>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var list = JsonSerializer.Deserialize<List<BuildPcSelectedItem>>(raw, JsonOptions)
                       ?? new List<BuildPcSelectedItem>();
            var dict = new Dictionary<string, BuildPcSelectedItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in list)
            {
                if (!string.IsNullOrWhiteSpace(item.SlotKey) && item.ProductId > 0)
                {
                    dict[item.SlotKey] = item;
                }
            }
            return dict;
        }
        catch
        {
            return new Dictionary<string, BuildPcSelectedItem>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static void SaveSelection(ISession session, Dictionary<string, BuildPcSelectedItem> items)
    {
        if (items.Count == 0)
        {
            session.Remove(BuildSessionKey);
            return;
        }

        session.SetString(BuildSessionKey, JsonSerializer.Serialize(items.Values.ToList(), JsonOptions));
    }
}
