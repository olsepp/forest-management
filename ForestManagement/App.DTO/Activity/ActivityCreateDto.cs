using System.ComponentModel.DataAnnotations;
using App.Domain;

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

    // User ID will be set from HttpContext.User (authenticated user)
    // Not included in the DTO for security

    // One or the other - mutually exclusive (for logging per cadaster OR forest stand)
    // If logging on cadaster level (no forest stands - agricultural land)
    public Guid? CadasterId { get; set; }
    
    // If logging on forest stand level
    public Guid? ForestStandId { get; set; }

    // Application status - only used when ActivityType is "toetuse taotlemine" (grant application)
    public EApplicationStatus? ApplicationStatus { get; set; }
}
