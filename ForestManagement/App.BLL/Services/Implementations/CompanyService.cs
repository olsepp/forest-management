using App.BLL.Services.Interfaces;
using App.DAL.UnitOfWork;
using App.Domain;
using App.DTO.Company;

namespace App.BLL.Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _uow;

    public CompanyService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<CompanyListDto>> GetAllAsync()
    {
        var companies = await _uow.Companies.GetAllAsync();
        return companies.Select(MapToListDto);
    }

    public async Task<CompanyDto?> GetByIdAsync(Guid id)
    {
        var company = await _uow.Companies.GetByIdAsync(id);
        return company == null ? null : MapToDto(company);
    }

    public async Task<CompanyDto?> GetWithPropertiesAsync(Guid id)
    {
        var company = await _uow.Companies.GetWithPropertiesAsync(id);
        return company == null ? null : MapToDto(company);
    }

    public async Task<CompanyDto> CreateAsync(CompanyCreateDto dto)
    {
        var entity = new Company
        {
            Name = dto.Name,
            RegistrationNumber = dto.RegistrationNumber
        };
        await _uow.Companies.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<CompanyDto?> UpdateAsync(Guid id, CompanyUpdateDto dto)
    {
        var entity = await _uow.Companies.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Name = dto.Name;
        entity.RegistrationNumber = dto.RegistrationNumber;
        await _uow.Companies.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await _uow.Companies.ExistsAsync(id)) return false;
        await _uow.Companies.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return true;
    }

    public Task<bool> ExistsAsync(Guid id) => _uow.Companies.ExistsAsync(id);

    // --- Mapping ---

    private static CompanyDto MapToDto(Company c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        RegistrationNumber = c.RegistrationNumber,
        PropertyCount = c.Properties?.Count ?? 0
    };

    private static CompanyListDto MapToListDto(Company c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        RegistrationNumber = c.RegistrationNumber
    };
}
