using App.DTO.Dashboard;

namespace App.BLL.Services.Interfaces;

public interface IDashboardService
{
    /// <summary>
    /// Get dashboard summary for a company
    /// </summary>
    Task<DashboardSummaryDto> GetSummaryByCompanyIdAsync(Guid companyId);
}