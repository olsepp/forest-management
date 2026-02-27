using App.Domain;

namespace App.DTO.LandProperty;

/// <summary>
/// Search and filter parameters for property list
/// </summary>
public class LandPropertySearchParams
{
    /// <summary>
    /// Search by property name, registration number, or cadastral number
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Filter by county
    /// </summary>
    public string? County { get; set; }

    /// <summary>
    /// Filter by property status (ACTIVE, INACTIVE, SOLD)
    /// </summary>
    public EPropertyStatus? Status { get; set; }

    /// <summary>
    /// Company ID for filtering properties by company
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// If true, only return ACTIVE properties (used for employee view)
    /// </summary>
    public bool ActiveOnly { get; set; }
}
