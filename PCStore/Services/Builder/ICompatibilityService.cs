using PCStore.ViewModels.Builder;

namespace PCStore.Services.Builder;

public interface ICompatibilityService
{
    CompatibilityReport Evaluate(IReadOnlyDictionary<string, BuildPcSlotRowViewModel> selectedRows);
    (string Status, string? Reason) EvaluateCandidate(
        string slotKey,
        BuildPcProductDto candidate,
        IReadOnlyDictionary<string, BuildPcSlotRowViewModel> currentSelectedRows);
}
