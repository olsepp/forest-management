using App.Domain.Identity;
using Base.Domain;
using App.Contracts.Enums;


namespace App.Domain;
public class WorkOrder : BaseEntity
{
    public Guid AssignedToId { get; set; }
    public Guid AssignedById { get; set; }
    public Guid? ForestStandId { get; set; }
    public Guid CadasterId { get; set; }
    public Guid ActivityTypeId { get; set; }

    public AppUser AssignedTo { get; set; } = default!;
    public AppUser AssignedBy { get; set; } = default!;
    public ForestStand? ForestStand { get; set; }
    public Cadaster Cadaster { get; set; } = default!;
    public ActivityType ActivityType { get; set; } = default!;
    public EOrderStatus Status { get; set; }
    public decimal Quantity { get; set; } // How much was done (e.g. 200)
    public string? Unit { get; set; } // Unit of quantity (e.g. "trees", "m³", "ha")
    public string? Notes { get; set; } // Optional free-text notes
    public DateTime CreatedAt { get; set; } // Date the activity was performed

}
