using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class ForestStand : BaseEntity
{
    [Range(0, 100)]
    public int Number { get; set; }
    
    // Value in hectare(ha)
    public decimal Area { get; set; }
    
    // Maht kokku
    // Value in densimeter(tm)
    public int TotalVolume { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
    
    public DateTime? ValidTo { get; set; } = null;
    
    // Foreign key to Cadaster
    public Guid CadasterId { get; set; }
    public Cadaster Cadaster { get; set; } = null!;

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}