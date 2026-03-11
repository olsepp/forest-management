using App.BLL.Services.Interfaces;
using App.DAL.UnitOfWork;
using App.Domain;
using App.DTO.LandProperty;

namespace App.BLL.Services.Implementations;

public class LandPropertyService : ILandPropertyService
{
    private readonly IUnitOfWork _uow;

    public LandPropertyService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<LandPropertyListDto>> GetAllAsync()
    {
        var props = await _uow.LandProperties.GetAllWithCompanyAsync();
        return props.Select(MapToListDto);
    }

    public async Task<LandPropertyDto?> GetByIdAsync(Guid id)
    {
        var prop = await _uow.LandProperties.GetWithCompanyAsync(id);
        return prop == null ? null : MapToDto(prop);
    }

    public async Task<IEnumerable<LandPropertyListDto>> SearchAsync(LandPropertySearchParams searchParams)
    {
        var props = await _uow.LandProperties.SearchAsync(searchParams);
        return props.Select(MapToListDto);
    }

    public async Task<LandPropertyDto> CreateAsync(LandPropertyCreateDto dto)
    {
        var entity = new LandProperty
        {
            Name = dto.Name,
            RegistrationNumber = dto.RegistrationNumber,
            County = dto.County,
            Parish = dto.Parish,
            Village = dto.Village,
            BoughtDate = dto.BoughtDate,
            SoldDate = dto.SoldDate,
            Status = dto.Status,
            CompanyId = dto.CompanyId
        };
        await _uow.LandProperties.AddAsync(entity);
        await _uow.SaveChangesAsync();
        // Reload with company for response
        var created = await _uow.LandProperties.GetWithCompanyAsync(entity.Id);
        return MapToDto(created!);
    }

    public async Task<LandPropertyDto?> UpdateAsync(Guid id, LandPropertyUpdateDto dto)
    {
        var entity = await _uow.LandProperties.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Name = dto.Name;
        entity.RegistrationNumber = dto.RegistrationNumber;
        entity.County = dto.County;
        entity.Parish = dto.Parish;
        entity.Village = dto.Village;
        entity.BoughtDate = dto.BoughtDate;
        entity.SoldDate = dto.SoldDate;
        entity.Status = dto.Status;
        entity.CompanyId = dto.CompanyId;

        await _uow.LandProperties.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
        var updated = await _uow.LandProperties.GetWithCompanyAsync(id);
        return MapToDto(updated!);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await _uow.LandProperties.ExistsAsync(id)) return false;
        await _uow.LandProperties.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return true;
    }

    public Task<bool> ExistsAsync(Guid id) => _uow.LandProperties.ExistsAsync(id);

    // --- Mapping ---

    private static LandPropertyDto MapToDto(LandProperty p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        RegistrationNumber = p.RegistrationNumber,
        County = p.County,
        Parish = p.Parish,
        Village = p.Village,
        BoughtDate = p.BoughtDate,
        SoldDate = p.SoldDate,
        Status = p.Status,
        CompanyId = p.CompanyId,
        CompanyName = p.Company?.Name ?? string.Empty,
        Cadasters = p.Cadasters.Select(c => new App.DTO.Cadaster.CadasterListDto
        {
            Id = c.Id,
            CadastralNumber = c.CadastralNumber,
            ForestArea = c.ForestArea,
            ForestStandCount = c.ForestStands?.Count ?? 0
        }).ToList()
    };

    private static LandPropertyListDto MapToListDto(LandProperty p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        RegistrationNumber = p.RegistrationNumber,
        County = p.County,
        Parish = p.Parish,
        Village = p.Village,
        BoughtDate = p.BoughtDate,
        SoldDate = p.SoldDate,
        Status = p.Status,
        CompanyId = p.CompanyId,
        CompanyName = p.Company?.Name ?? string.Empty,
        Cadasters = p.Cadasters?.Select(c => new LandPropertyCadasterLinkDto
        {
            Id = c.Id,
            CadastralNumber = c.CadastralNumber
        }).ToList() ?? new List<LandPropertyCadasterLinkDto>()
    };
}
