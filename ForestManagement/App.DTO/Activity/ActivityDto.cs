
using App.Contracts.Enums;

namespace App.DTO.Activity;

/// <summary>
/// Full activity details response DTO with navigation info
/// </summary>
public class ActivityDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public DateTime Date { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserFirstName { get; set; }
    public string? UserLastName { get; set; }
    public Guid ActivityTypeId { get; set; }
    public string ActivityTypeName { get; set; } = string.Empty;
    
    // Cadaster context (nullable - activity may be on forest stand instead)
    public Guid? CadasterId { get; set; }
    public string? CadasterCadastralNumber { get; set; }
    
    // Forest stand context (nullable - activity may be on cadaster only)
    public Guid? ForestStandId { get; set; }
    public int? ForestStandNumber { get; set; }
    
    // Property context (for display)
    public Guid? LandPropertyId { get; set; }
    public string? LandPropertyName { get; set; }
    
    // Application status (only used for grant applications)
    public EApplicationStatus? ApplicationStatus { get; set; }
}
