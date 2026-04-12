using App.DTO.Activity;

namespace App.DTO.Dashboard;

/// <summary>
/// DTO for dashboard summary endpoint
/// </summary>
public class DashboardSummaryDto
{
    public int TotalProperties { get; set; }
    public int TotalActiveProperties { get; set; }
    public int TotalCadasters { get; set; }
    public List<ActivityCountByDayDto> ActivityCountsByDay { get; set; } = new();
    public List<ActivityListDto> RecentActivities { get; set; } = new();
}

/// <summary>
/// DTO for activity count by day
/// </summary>
public class ActivityCountByDayDto
{
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}