using System.ComponentModel.DataAnnotations;
using App.Domain;

namespace WebApp.Models;

public class CadasterCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    public string CadastralNumber { get; set; } = string.Empty;

    public decimal? ForestArea { get; set; }
    public decimal? ArableArea { get; set; }
    public decimal? GrasslandArea { get; set; }
    public decimal? YardArea { get; set; }
    public decimal? BuildingFootprintArea { get; set; }
    public decimal? UnderwaterArea { get; set; }
    public decimal? OtherArea { get; set; }

    [Range(0, 4)]
    public int? SoilQualityIndex { get; set; }

    public int? CalculatedVolume { get; set; }
    public decimal? VolumeGrowth { get; set; }

    [Required]
    public Guid LandPropertyId { get; set; }
}
