using PCStore.ViewModels.Builder;

namespace PCStore.Repositories.Builder;

public interface IBuildPcRepository
{
    Task<List<BuildPcProductDto>> GetAvailableProductsBySlotAsync(
        BuildPcSlotDefinition slot,
        string? keyword = null,
        CancellationToken cancellationToken = default);

    Task<Dictionary<long, BuildPcProductDto>> GetProductsByIdsAsync(
        IEnumerable<long> productIds,
        CancellationToken cancellationToken = default);

    Task<BuildPcProductDto?> GetProductByIdAsync(
        long productId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, int>> GetAvailableCountsBySlotAsync(
        CancellationToken cancellationToken = default);
}
