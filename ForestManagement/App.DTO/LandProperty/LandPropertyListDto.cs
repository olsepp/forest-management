using App.Domain;

namespace App.DTO.LandProperty;

/// <summary>
/// Table display DTO for property list with cadastral numbers
/// </summary>
public class LandPropertyListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RegistrationNumber { get; set; }
    public string County { get; set; } = string.Empty;
    public EPropertyStatus Status { get; set; }
    
    // Cadastral numbers that belong to this property
    public List<string> CadastralNumbers { get; set; } = new();
}
