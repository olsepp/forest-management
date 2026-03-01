using System.ComponentModel.DataAnnotations;

namespace App.DTO.Company;

/// <summary>
/// Request DTO for updating an existing company
/// </summary>
public class CompanyUpdateDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int RegistrationNumber { get; set; }
}
