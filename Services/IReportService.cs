using PharmaLink.ViewModels;

namespace PharmaLink.Services;

public interface IReportService
{
    Task<ReportViewModel> GetAdminReportAsync();
    Task<PharmacistReportViewModel> GetPharmacistReportAsync(int pharmacyId);
}