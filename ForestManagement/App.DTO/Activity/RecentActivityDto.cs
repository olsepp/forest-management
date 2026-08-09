namespace App.DTO.Activity;

/// <summary>
/// DTO for displaying recent activities (5 most recent)
/// </summary>
public class RecentActivityDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public DateTime Date { get; set; }
    public string ActivityTypeName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserFirstName { get; set; }
    public string? UserLastName { get; set; }
    
    // Context for where the activity was logged
    public Guid? CadasterId { get; set; }
    public Guid? ForestStandId { get; set; }
    public string? CadasterCadastralNumber { get; set; }
    public int? ForestStandNumber { get; set; }
}
