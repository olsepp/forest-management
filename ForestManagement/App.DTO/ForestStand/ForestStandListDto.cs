namespace App.DTO.ForestStand;

/// <summary>
/// Table display DTO for forest stand list within a cadaster
/// </summary>
public class ForestStandListDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public decimal Area { get; set; }
    public int TotalVolume { get; set; }
    public bool IsActive { get; set; }
}
