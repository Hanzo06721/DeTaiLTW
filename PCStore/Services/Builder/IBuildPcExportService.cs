using PCStore.ViewModels.Builder;

namespace PCStore.Services.Builder;

public interface IBuildPcExportService
{
    byte[] ExportPdf(BuildPcPageViewModel model);
    byte[] ExportExcel(BuildPcPageViewModel model);
}
