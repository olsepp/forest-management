using App.BLL.Services.Interfaces;
using App.DTO.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

[Route("api/dashboard")]
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    /// <summary>
    /// Get dashboard summary for a company
    /// </summary>
    [HttpGet("{companyId:guid}/summary")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(Guid companyId)
    {
        var summary = await _service.GetSummaryByCompanyIdAsync(companyId);
        return Ok(summary);
    }
}