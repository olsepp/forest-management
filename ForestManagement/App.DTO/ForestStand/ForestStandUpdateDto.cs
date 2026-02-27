using System.ComponentModel.DataAnnotations;

namespace App.DTO.ForestStand;

/// <summary>
/// Request DTO for updating an existing forest stand
/// </summary>
public class ForestStandUpdateDto
{
    [Required]
    public Guid Id { get; set; }

    [Range(0, 100)]
    public int Number { get; set; }

    public decimal Area { get; set; }

    public int TotalVolume { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    [Required]
    public Guid CadasterId { get; set; }
}
