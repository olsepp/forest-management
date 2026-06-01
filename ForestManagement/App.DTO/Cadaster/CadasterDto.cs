using App.DTO.ForestStand;

namespace App.DTO.Cadaster;

/// <summary>
/// Full cadaster details response DTO with forest stands and recent activities
/// </summary>
public class CadasterDto
{
    public Guid Id { get; set; }
    public string CadastralNumber { get; set; } = string.Empty;
    public decimal? ForestArea { get; set; }
    public decimal? ArableArea { get; set; }
    public decimal? GrasslandArea { get; set; }
    public decimal? YardArea { get; set; }
    public decimal? BuildingFootprintArea { get; set; }
    public decimal? UnderwaterArea { get; set; }
    public decimal? OtherArea { get; set; }
    public int? SoilQualityIndex { get; set; }
    public int? CalculatedVolume { get; set; }
    public decimal? VolumeGrowth { get; set; }
    public Guid LandPropertyId { get; set; }
    public string LandPropertyName { get; set; } = string.Empty;
    public bool LandPropertyIsFsc { get; set; }
    
    // Related forest stands
    public ICollection<ForestStandListDto> ForestStands { get; set; } = new List<ForestStandListDto>();
    
    // Recent activities (5 most recent)
    public ICollection<Activity.RecentActivityDto> RecentActivities { get; set; } = new List<Activity.RecentActivityDto>();
}
