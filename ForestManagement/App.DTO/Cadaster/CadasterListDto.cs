namespace App.DTO.Cadaster;

/// <summary>
/// Table display DTO for cadaster with forest stand count
/// </summary>
public class CadasterListDto
{
    public Guid Id { get; set; }
    public string CadastralNumber { get; set; } = string.Empty;
    public decimal? ForestArea { get; set; }
    public int ForestStandCount { get; set; }
}
