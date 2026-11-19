using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using PCStore.Data;
using PCStore.Models;
using PCStore.ViewModels.Builder;

namespace PCStore.Repositories.Builder;

public class BuildPcRepository : IBuildPcRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public BuildPcRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<BuildPcProductDto>> GetAvailableProductsBySlotAsync(
        BuildPcSlotDefinition slot,
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var categories = await LoadCategoriesAsync(conn, cancellationToken);
        var matchedCategoryIds = ResolveCategoryIdsForSlot(categories, slot);
        if (matchedCategoryIds.Count == 0)
        {
            return new List<BuildPcProductDto>();
        }

        var sql = new StringBuilder(@"
            SELECT p.id, p.sku, p.[name], p.price, p.stock_quantity, p.thumbnail, p.category_id,
                   ISNULL(c.[name], N'') AS category_name
            FROM dbo.products p
            LEFT JOIN dbo.categories c ON c.id = p.category_id
            WHERE p.product_type = N'COMPONENT'
              AND p.stock_quantity > 0
              AND p.category_id IN (");

        await using var cmd = new SqlCommand { Connection = conn };
        for (var i = 0; i < matchedCategoryIds.Count; i++)
        {
            if (i > 0) sql.Append(", ");
            var paramName = $"@cat{i}";
            sql.Append(paramName);
            cmd.Parameters.Add(paramName, SqlDbType.Int).Value = matchedCategoryIds[i];
        }
        sql.Append(")");

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            sql.Append(@"
              AND (
                    p.[name] LIKE @kw
                 OR p.sku LIKE @kw
                 OR EXISTS (
                        SELECT 1
                        FROM dbo.product_attributes pa
                        WHERE pa.product_id = p.id
                          AND pa.[value] LIKE @kw
                    )
              )");
            cmd.Parameters.Add("@kw", SqlDbType.NVarChar, 300).Value = $"%{keyword.Trim()}%";
        }

        sql.Append(" ORDER BY p.price ASC, p.[name] ASC;");
        cmd.CommandText = sql.ToString();

        var products = await ReadProductsAsync(cmd, cancellationToken);
        await AttachAttributesAsync(conn, products, cancellationToken);
        return products;
    }

    public async Task<Dictionary<long, BuildPcProductDto>> GetProductsByIdsAsync(
        IEnumerable<long> productIds,
        CancellationToken cancellationToken = default)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<long, BuildPcProductDto>();
        }

        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var sql = new StringBuilder(@"
            SELECT p.id, p.sku, p.[name], p.price, p.stock_quantity, p.thumbnail, p.category_id,
                   ISNULL(c.[name], N'') AS category_name
            FROM dbo.products p
            LEFT JOIN dbo.categories c ON c.id = p.category_id
            WHERE p.id IN (");

        await using var cmd = new SqlCommand { Connection = conn };
        for (var i = 0; i < ids.Count; i++)
        {
            if (i > 0) sql.Append(", ");
            var paramName = $"@id{i}";
            sql.Append(paramName);
            cmd.Parameters.Add(paramName, SqlDbType.BigInt).Value = ids[i];
        }
        sql.Append(");");
        cmd.CommandText = sql.ToString();

        var products = await ReadProductsAsync(cmd, cancellationToken);
        await AttachAttributesAsync(conn, products, cancellationToken);
        return products.ToDictionary(p => p.Id);
    }

    public async Task<BuildPcProductDto?> GetProductByIdAsync(
        long productId,
        CancellationToken cancellationToken = default)
    {
        var map = await GetProductsByIdsAsync(new[] { productId }, cancellationToken);
        return map.GetValueOrDefault(productId);
    }

    public async Task<Dictionary<string, int>> GetAvailableCountsBySlotAsync(
        CancellationToken cancellationToken = default)
    {
        var result = BuildPcSlots.All.ToDictionary(s => s.Key, _ => 0, StringComparer.OrdinalIgnoreCase);

        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var categories = await LoadCategoriesAsync(conn, cancellationToken);

        var countByCategory = new Dictionary<int, int>();
        const string sql = @"
            SELECT category_id, COUNT(*) AS cnt
            FROM dbo.products
            WHERE product_type = N'COMPONENT'
              AND stock_quantity > 0
              AND category_id IS NOT NULL
            GROUP BY category_id;";

        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            countByCategory[reader.GetInt32(0)] = reader.GetInt32(1);
        }

        foreach (var slot in BuildPcSlots.All)
        {
            var catIds = ResolveCategoryIdsForSlot(categories, slot);
            result[slot.Key] = catIds.Sum(id => countByCategory.GetValueOrDefault(id));
        }

        return result;
    }

    private static async Task<List<Category>> LoadCategoriesAsync(
        SqlConnection conn,
        CancellationToken cancellationToken)
    {
        var categories = new List<Category>();
        const string sql = "SELECT id, [name], parent_id FROM dbo.categories;";

        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            categories.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                ParentId = reader.IsDBNull(2) ? null : reader.GetInt32(2)
            });
        }

        return categories;
    }

    private static async Task<List<BuildPcProductDto>> ReadProductsAsync(
        SqlCommand cmd,
        CancellationToken cancellationToken)
    {
        var list = new List<BuildPcProductDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new BuildPcProductDto
            {
                Id = reader.GetInt64(0),
                Sku = reader.GetString(1),
                Name = reader.GetString(2),
                Price = reader.GetDecimal(3),
                StockQuantity = reader.GetInt32(4),
                Thumbnail = reader.IsDBNull(5) ? null : reader.GetString(5),
                CategoryId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                CategoryName = reader.GetString(7)
            });
        }

        return list;
    }

    private static async Task AttachAttributesAsync(
        SqlConnection conn,
        List<BuildPcProductDto> products,
        CancellationToken cancellationToken)
    {
        if (products.Count == 0) return;

        var byId = products.ToDictionary(p => p.Id);
        var sql = new StringBuilder(@"
            SELECT pa.product_id, pa.attribute_id, a.[name], pa.[value]
            FROM dbo.product_attributes pa
            INNER JOIN dbo.attributes a ON a.id = pa.attribute_id
            WHERE pa.product_id IN (");

        await using var cmd = new SqlCommand { Connection = conn };
        for (var i = 0; i < products.Count; i++)
        {
            if (i > 0) sql.Append(", ");
            var paramName = $"@pid{i}";
            sql.Append(paramName);
            cmd.Parameters.Add(paramName, SqlDbType.BigInt).Value = products[i].Id;
        }
        sql.Append(") ORDER BY a.id ASC;");
        cmd.CommandText = sql.ToString();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var productId = reader.GetInt64(0);
            if (byId.TryGetValue(productId, out var product))
            {
                product.Attributes.Add(new BuildPcAttributeItem
                {
                    AttributeId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Value = reader.GetString(3)
                });
            }
        }
    }

    private static List<int> ResolveCategoryIdsForSlot(
        List<Category> categories,
        BuildPcSlotDefinition slot)
    {
        var normalizedKeywords = slot.CategoryKeywords
            .Select(NormalizeText)
            .Where(k => !string.IsNullOrEmpty(k))
            .Distinct()
            .ToArray();

        var matched = new HashSet<int>();
        foreach (var cat in categories)
        {
            var normalizedName = NormalizeText(cat.Name);
            if (MatchesAnyKeyword(normalizedName, normalizedKeywords))
            {
                matched.Add(cat.Id);
            }
        }

        // Bao gồm các danh mục con thuộc danh mục đã khớp (vd: CPU -> CPU Intel, CPU AMD)
        var added = true;
        while (added)
        {
            added = false;
            foreach (var cat in categories)
            {
                if (cat.ParentId.HasValue && matched.Contains(cat.ParentId.Value) && matched.Add(cat.Id))
                {
                    added = true;
                }
            }
        }

        return matched.ToList();
    }

    private static bool MatchesAnyKeyword(string normalizedText, string[] normalizedKeywords)
    {
        if (string.IsNullOrEmpty(normalizedText)) return false;

        foreach (var kw in normalizedKeywords)
        {
            if (normalizedText == kw) return true;
            if (Regex.IsMatch(normalizedText, $@"(^|[^a-z0-9]){Regex.Escape(kw)}($|[^a-z0-9])"))
                return true;
        }

        return false;
    }

    private static string NormalizeText(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var formD = input.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }

        return Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC), @"\s+", " ").Trim();
    }
}
