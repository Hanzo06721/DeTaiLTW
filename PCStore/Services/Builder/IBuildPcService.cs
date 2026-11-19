using Microsoft.AspNetCore.Http;
using PCStore.ViewModels.Builder;

namespace PCStore.Services.Builder;

public record BuildPcOperationResult(bool Success, string Message, int TransferredCount = 0);

public interface IBuildPcService
{
    Task<BuildPcPageViewModel> GetBuildPageAsync(ISession session, CancellationToken cancellationToken = default);

    Task<List<BuildPcCandidateDto>> SearchSlotProductsAsync(
        ISession session,
        string slotKey,
        string? keyword = null,
        CancellationToken cancellationToken = default);

    Task<BuildPcOperationResult> SelectComponentAsync(
        ISession session,
        string slotKey,
        long productId,
        int quantity = 1,
        CancellationToken cancellationToken = default);

    Task<BuildPcOperationResult> UpdateQuantityAsync(
        ISession session,
        string slotKey,
        int quantity,
        CancellationToken cancellationToken = default);

    BuildPcOperationResult RemoveComponent(ISession session, string slotKey);

    BuildPcOperationResult ClearBuild(ISession session);

    Task<BuildPcOperationResult> TransferToCartAsync(ISession session, CancellationToken cancellationToken = default);
}
