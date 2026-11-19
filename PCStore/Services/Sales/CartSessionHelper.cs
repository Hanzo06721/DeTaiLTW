using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PCStore.ViewModels.Sales;

namespace PCStore.Services.Sales;

/// <summary>
/// Helper đọc/ghi giỏ hàng tạm trong Session để dùng chung giữa NV3 (Build PC) và NV4 (Cart / Checkout).
/// </summary>
public static class CartSessionHelper
{
    public const string CartSessionKey = "PCStore.Cart";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static List<CartSessionItem> GetCart(ISession session)
    {
        var raw = session.GetString(CartSessionKey);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new List<CartSessionItem>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<CartSessionItem>>(raw, JsonOptions)
                   ?? new List<CartSessionItem>();
        }
        catch
        {
            return new List<CartSessionItem>();
        }
    }

    public static void SaveCart(ISession session, List<CartSessionItem> items)
    {
        if (items.Count == 0)
        {
            session.Remove(CartSessionKey);
            return;
        }

        session.SetString(CartSessionKey, JsonSerializer.Serialize(items, JsonOptions));
    }

    public static int GetTotalQuantity(ISession session) =>
        GetCart(session).Sum(i => Math.Max(0, i.Quantity));

    public static void MergeItems(ISession session, IEnumerable<CartSessionItem> incomingItems)
    {
        var cart = GetCart(session);
        foreach (var item in incomingItems)
        {
            if (item.Quantity <= 0) continue;

            var existing = cart.FirstOrDefault(x => x.ProductId == item.ProductId);
            if (existing is not null)
            {
                existing.UnitPrice = item.UnitPrice;
                existing.Name = item.Name;
                existing.Sku = item.Sku;
                existing.Thumbnail = item.Thumbnail;
                existing.StockQuantity = item.StockQuantity;
                var newQty = existing.Quantity + item.Quantity;
                existing.Quantity = item.StockQuantity > 0 ? Math.Min(newQty, item.StockQuantity) : newQty;
            }
            else
            {
                cart.Add(new CartSessionItem
                {
                    ProductId = item.ProductId,
                    Sku = item.Sku,
                    Name = item.Name,
                    Thumbnail = item.Thumbnail,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.StockQuantity > 0 ? Math.Min(item.Quantity, item.StockQuantity) : item.Quantity,
                    StockQuantity = item.StockQuantity,
                    Source = item.Source
                });
            }
        }

        SaveCart(session, cart);
    }
}
