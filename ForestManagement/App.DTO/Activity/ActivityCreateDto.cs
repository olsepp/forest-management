using System.ComponentModel.DataAnnotations;
using App.Contracts.Enums;

namespace App.DTO.Activity;

/// <summary>
/// Request DTO for creating a new activity (logging activity per cadaster or forest stand)
/// </summary>
public class ActivityCreateDto
{
    [Required]
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public string? Notes { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid ActivityTypeId { get; set; }

    // User ID is normally set from HttpContext.User (authenticated user).
    // Admins may set this to log an activity on behalf of another user;
    // the service ignores any value supplied by a non-admin caller.
    public Guid? UserId { get; set; }

    // When logging on cadaster: forest stand will be null
    // When logging on forest stand: both fields will have a value
    // If logging on cadaster level (no forest stands - agricultural land)
    public Guid? CadasterId { get; set; }
    
    // If logging on forest stand level
    public Guid? ForestStandId { get; set; }

    // Application status - only used when ActivityType is "toetuse taotlemine" (grant application)
    public EApplicationStatus? ApplicationStatus { get; set; }
}
