using System.ComponentModel.DataAnnotations;
using App.Domain;

namespace App.DTO.Activity;

/// <summary>
/// Request DTO for updating an existing activity
/// </summary>
public class ActivityUpdateDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public string? Notes { get; set; }

    public DateTime Date { get; set; }

    [Required]
    public Guid ActivityTypeId { get; set; }

    // One or the other - mutually exclusive
    public Guid? ForestStandId { get; set; }
    public Guid? CadasterId { get; set; }

    // Application status - only used when ActivityType is "toetuse taotlemine" (grant application)
    public EApplicationStatus? ApplicationStatus { get; set; }
}
