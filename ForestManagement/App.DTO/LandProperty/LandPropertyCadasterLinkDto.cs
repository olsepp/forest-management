namespace App.DTO.LandProperty;

/// <summary>
/// Lightweight cadaster item for linking from land property lists.
/// </summary>
public class LandPropertyCadasterLinkDto
{
    public Guid Id { get; set; }
    public string CadastralNumber { get; set; } = string.Empty;
}
