using App.BLL.Services.Interfaces;
using App.Contracts.Enums;
using App.DAL.UnitOfWork;
using App.DTO.Activity;
using App.DTO.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace App.BLL.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow) => _uow = uow;

    public async Task<DashboardSummaryDto> GetSummaryByCompanyIdAsync(Guid companyId)
    {
        // Get all land properties for the company with cadasters
        var properties = await _uow.LandProperties.GetAllWithCadastersAsync();
        var companyProperties = properties.Where(p => p.CompanyId == companyId).ToList();

        var totalProperties = companyProperties.Count;
        var totalActiveProperties = companyProperties.Count(p => p.Status == EPropertyStatus.Active);
        var totalCadasters = companyProperties.Sum(p => p.Cadasters?.Count ?? 0);

        // Get activity counts by day (last 30 days)
        var thirtyDaysAgo = DateTime.Now.AddDays(-30).Date;
        var activities = await _uow.Activities.GetByCompanyIdAsync(companyId);
        
        var activityCountsByDay = activities
            .Where(a => a.Date.Date >= thirtyDaysAgo)
            .GroupBy(a => a.Date.Date)
            .Select(g => new ActivityCountByDayDto
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        // Get recent activities (top 5)
        var recentActivities = activities
            .OrderByDescending(a => a.Date)
            .Take(5)
            .Select(a => new ActivityListDto
            {
                Id = a.Id,
                Description = a.Description,
                Quantity = a.Quantity,
                Unit = a.Unit,
                Date = a.Date,
                ActivityTypeName = a.ActivityType?.ActivityTypeName ?? string.Empty,
                UserName = a.User?.UserName ?? string.Empty,
                CadasterCadastralNumber = a.Cadaster?.CadastralNumber,
                ForestStandNumber = a.ForestStand?.Number,
                LocationDescription = a.Cadaster?.LandProperty != null 
                    ? $"{a.Cadaster.LandProperty.Name}, {a.Cadaster.LandProperty.County}"
                    : null,
                ApplicationStatus = a.ApplicationStatus
            })
            .ToList();

        return new DashboardSummaryDto
        {
            TotalProperties = totalProperties,
            TotalActiveProperties = totalActiveProperties,
            TotalCadasters = totalCadasters,
            ActivityCountsByDay = activityCountsByDay,
            RecentActivities = recentActivities
        };
    }
}