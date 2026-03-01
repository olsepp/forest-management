using App.Contracts.Enums;

namespace App.DTO.Activity;

/// <summary>
/// Table display DTO for activity list
/// </summary>
public class ActivityListDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public DateTime Date { get; set; }
    public string ActivityTypeName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    
    // Context for where the activity was logged
    public string? CadasterCadastralNumber { get; set; }
    public int? ForestStandNumber { get; set; }
    public string? LocationDescription { get; set; }
    
    // Application status (for grant applications)
    public EApplicationStatus? ApplicationStatus { get; set; }
}
