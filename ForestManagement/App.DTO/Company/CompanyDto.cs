namespace App.DTO.Company;

/// <summary>
/// Full company details response DTO
/// </summary>
public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RegistrationNumber { get; set; }
    public int PropertyCount { get; set; }
    public int UserCount { get; set; }
}
