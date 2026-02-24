using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class Cadaster : BaseEntity
{
    [Required] public string CadastralNumber { get; set; } = default!;
    
    // Value in hectare(ha)
    public decimal? ForestArea { get; set; }
    
    // Value in hectare(ha)
    public decimal? ArableArea { get; set; }
    
    // Value in hectare(ha)
    public decimal? GrasslandArea { get; set; }
    
    // Value in hectare(ha)
    public decimal? YardArea { get; set; }
    
    // Value in hectare(ha)
    public decimal? BuildingFootprintArea { get; set; }
    
    // Value in hectare(ha)
    public decimal? UnderwaterArea { get; set; }
    
    // Value in hectare(ha)
    public decimal? OtherArea { get; set; }
    
    // Boniteet
    [Range(0, 4)]
    public int? SoilQualityIndex { get; set; }
    
    // Value in densimeter(tihumeeter)
    public int? CalculatedVolume { get; set; }
    
    // Value in densimeter/year(tihumeetrit aastas)
    public decimal? VolumeGrowth { get; set; }
    
    // Foreign key to Property
    public Guid LandPropertyId { get; set; }
    public LandProperty LandProperty { get; set; } = null!;

    public ICollection<ForestStand> ForestStands { get; set; } = new List<ForestStand>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();

}