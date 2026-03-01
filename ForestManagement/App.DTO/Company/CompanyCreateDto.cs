using System.ComponentModel.DataAnnotations;

namespace App.DTO.Company;

/// <summary>
/// Request DTO for creating a new company
/// </summary>
public class CompanyCreateDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int RegistrationNumber { get; set; }
}
