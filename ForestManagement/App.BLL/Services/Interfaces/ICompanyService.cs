using App.DTO.Company;

namespace App.BLL.Services.Interfaces;

public interface ICompanyService
{
    Task<IEnumerable<CompanyListDto>> GetAllAsync();
    Task<CompanyDto?> GetByIdAsync(Guid id);
    Task<CompanyDto?> GetWithPropertiesAsync(Guid id);
    Task<CompanyDto> CreateAsync(CompanyCreateDto dto);
    Task<CompanyDto?> UpdateAsync(Guid id, CompanyUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
