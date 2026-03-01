using App.Contracts.Enums;

namespace App.DTO.LandProperty;

/// <summary>
/// Full property details response DTO with related cadasters
/// </summary>
public class LandPropertyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RegistrationNumber { get; set; }
    public string County { get; set; } = string.Empty;
    public string Parish { get; set; } = string.Empty;
    public string Village { get; set; } = string.Empty;
    public DateTime? BoughtDate { get; set; }
    public DateTime? SoldDate { get; set; }
    public EPropertyStatus Status { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    
    // Related cadasters
    public ICollection<Cadaster.CadasterListDto> Cadasters { get; set; } = new List<Cadaster.CadasterListDto>();
}
