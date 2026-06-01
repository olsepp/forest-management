using System.ComponentModel.DataAnnotations;
using App.Contracts.Enums;

namespace App.DTO.LandProperty;

/// <summary>
/// Request DTO for creating a new property
/// </summary>
public class LandPropertyCreateDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int RegistrationNumber { get; set; }

    [Required]
    [MaxLength(255)]
    public string County { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Parish { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Village { get; set; } = string.Empty;

    public DateTime? BoughtDate { get; set; }

    public DateTime? SoldDate { get; set; }

    public bool IsFsc { get; set; }

    [Required]
    public EPropertyStatus Status { get; set; }

    [Required]
    public Guid CompanyId { get; set; }
}
