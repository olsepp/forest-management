using App.BLL.Services.Interfaces;
using App.DAL.UnitOfWork;
using App.Domain;
using App.DTO.Activity;
using App.DTO.ForestStand;

namespace App.BLL.Services.Implementations;

public class ForestStandService : IForestStandService
{
    private readonly IUnitOfWork _uow;

    public ForestStandService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<ForestStandListDto>> GetAllAsync()
    {
        var stands = await _uow.ForestStands.GetAllAsync();
        return stands.Select(MapToListDto);
    }

    public async Task<ForestStandDto?> GetByIdAsync(Guid id)
    {
        var stand = await _uow.ForestStands.GetWithCadasterAsync(id);
        return stand == null ? null : MapToDto(stand);
    }

    public async Task<IEnumerable<ForestStandListDto>> GetByCadasterIdAsync(Guid cadasterId)
    {
        var stands = await _uow.ForestStands.GetByCadasterIdAsync(cadasterId);
        return stands.Select(MapToListDto);
    }

    public async Task<IEnumerable<ForestStandListDto>> GetActiveAsync()
    {
        var stands = await _uow.ForestStands.GetActiveAsync();
        return stands.Select(MapToListDto);
    }

    public async Task<ForestStandDto> CreateAsync(ForestStandCreateDto dto)
    {
        var entity = new ForestStand
        {
            Number = dto.Number,
            Area = dto.Area,
            TotalVolume = dto.TotalVolume,
            IsActive = dto.IsActive,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            CadasterId = dto.CadasterId
        };
        await _uow.ForestStands.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<ForestStandDto?> UpdateAsync(Guid id, ForestStandUpdateDto dto)
    {
        var entity = await _uow.ForestStands.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Number = dto.Number;
        entity.Area = dto.Area;
        entity.TotalVolume = dto.TotalVolume;
        entity.IsActive = dto.IsActive;
        entity.ValidFrom = dto.ValidFrom;
        entity.ValidTo = dto.ValidTo;
        entity.CadasterId = dto.CadasterId;

        await _uow.ForestStands.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await _uow.ForestStands.ExistsAsync(id)) return false;
        await _uow.ForestStands.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return true;
    }

    public Task<bool> ExistsAsync(Guid id) => _uow.ForestStands.ExistsAsync(id);

    // --- Mapping ---

    private static ForestStandDto MapToDto(ForestStand fs) => new()
    {
        Id = fs.Id,
        Number = fs.Number,
        Area = fs.Area,
        TotalVolume = fs.TotalVolume,
        IsActive = fs.IsActive,
        ValidFrom = fs.ValidFrom,
        ValidTo = fs.ValidTo,
        CadasterId = fs.CadasterId,
        CadasterCadastralNumber = fs.Cadaster?.CadastralNumber ?? string.Empty,
        LandPropertyId = fs.Cadaster?.LandPropertyId ?? Guid.Empty,
        LandPropertyName = fs.Cadaster?.LandProperty?.Name ?? string.Empty,
        RecentActivities = fs.Activities?.Take(5).Select(a => new RecentActivityDto
        {
            Id = a.Id,
            Description = a.Description,
            Quantity = a.Quantity,
            Unit = a.Unit,
            Date = a.Date,
            ForestStandNumber = fs.Number,
            ActivityTypeName = a.ActivityType?.ActivityTypeName ?? string.Empty,
            UserName = a.User?.UserName ?? string.Empty
        }).ToList() ?? new List<RecentActivityDto>()
    };

    private static ForestStandListDto MapToListDto(ForestStand fs) => new()
    {
        Id = fs.Id,
        Number = fs.Number,
        Area = fs.Area,
        TotalVolume = fs.TotalVolume,
        IsActive = fs.IsActive
    };
}
