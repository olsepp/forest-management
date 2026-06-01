using App.DTO.Activity;

namespace App.DTO.ForestStand;

/// <summary>
/// Full forest stand details response DTO
/// </summary>
public class ForestStandDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public decimal Area { get; set; }
    public int TotalVolume { get; set; }
    public bool IsActive { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public Guid CadasterId { get; set; }
    public string CadasterCadastralNumber { get; set; } = string.Empty;
    public Guid LandPropertyId { get; set; }
    public string LandPropertyName { get; set; } = string.Empty;
    public bool LandPropertyIsFsc { get; set; }
    
    // Recent activities (5 most recent)
    public ICollection<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
}
