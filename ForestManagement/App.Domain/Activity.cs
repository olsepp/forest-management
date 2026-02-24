using App.Domain.Identity;
using Base.Domain;


namespace App.Domain;
public class Activity : BaseEntity
{
    public string Description { get; set; } = null!;
    public decimal Quantity { get; set; } // How much was done (e.g. 200)
    public string? Unit { get; set; } // Unit of quantity (e.g. "trees", "m³", "ha")
    public string? Notes { get; set; } // Optional free-text notes
    public DateTime Date { get; set; } // Date the activity was performed

    
    // Identity uses Guid for user IDs
    public Guid UserId { get; set; }
    public Guid ActivityTypeId { get; set; }
    
    // Nullable foreign keys - either ForestStandId OR CadasterId must be set.
    // CadasterId is only used when the cadastral unit has no forest stands (agricultural land).
    public Guid? ForestStandId { get; set; }
    public Guid? CadasterId { get; set; }
    
    // Application status - only used when ActivityType is "toetuse taotlemine" (grant application)
    public EApplicationStatus? ApplicationStatus { get; set; }
    
    // Navigation properties
    public AppUser User { get; set; } = default!;
    public ActivityType ActivityType { get; set; } = default!;
    public ForestStand? ForestStand { get; set; }
    public Cadaster? Cadaster { get; set; }
}
