namespace App.DTO.Company;

/// <summary>
/// Minimal company info for dropdown selection
/// </summary>
public class CompanyListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RegistrationNumber { get; set; }
}
