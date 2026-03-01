using App.BLL.Services.Interfaces;
using App.DAL.UnitOfWork;
using App.Domain;
using App.DTO.Cadaster;
using App.DTO.Activity;

namespace App.BLL.Services.Implementations;

public class CadasterService : ICadasterService
{
    private readonly IUnitOfWork _uow;

    public CadasterService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<CadasterListDto>> GetAllAsync()
    {
        var cadasters = await _uow.Cadasters.GetAllWithForestStandsAsync();
        return cadasters.Select(MapToListDto);
    }

    public async Task<CadasterDto?> GetByIdAsync(Guid id)
    {
        var cadaster = await _uow.Cadasters.GetWithLandPropertyAsync(id);
        return cadaster == null ? null : MapToDto(cadaster);
    }

    public async Task<IEnumerable<CadasterListDto>> GetByLandPropertyIdAsync(Guid landPropertyId)
    {
        var cadasters = await _uow.Cadasters.GetByLandPropertyIdAsync(landPropertyId);
        return cadasters.Select(MapToListDto);
    }

    public async Task<CadasterDto> CreateAsync(CadasterCreateDto dto)
    {
        var entity = new Cadaster
        {
            CadastralNumber = dto.CadastralNumber,
            ForestArea = dto.ForestArea,
            ArableArea = dto.ArableArea,
            GrasslandArea = dto.GrasslandArea,
            YardArea = dto.YardArea,
            BuildingFootprintArea = dto.BuildingFootprintArea,
            UnderwaterArea = dto.UnderwaterArea,
            OtherArea = dto.OtherArea,
            SoilQualityIndex = dto.SoilQualityIndex,
            CalculatedVolume = dto.CalculatedVolume,
            VolumeGrowth = dto.VolumeGrowth,
            LandPropertyId = dto.LandPropertyId
        };
        await _uow.Cadasters.AddAsync(entity);
        await _uow.SaveChangesAsync();
        // Reload with land property for response
        var created = await _uow.Cadasters.GetWithLandPropertyAsync(entity.Id);
        return MapToDto(created!);
    }

    public async Task<CadasterDto?> UpdateAsync(Guid id, CadasterUpdateDto dto)
    {
        var entity = await _uow.Cadasters.GetByIdAsync(id);
        if (entity == null) return null;

        entity.CadastralNumber = dto.CadastralNumber;
        entity.ForestArea = dto.ForestArea;
        entity.ArableArea = dto.ArableArea;
        entity.GrasslandArea = dto.GrasslandArea;
        entity.YardArea = dto.YardArea;
        entity.BuildingFootprintArea = dto.BuildingFootprintArea;
        entity.UnderwaterArea = dto.UnderwaterArea;
        entity.OtherArea = dto.OtherArea;
        entity.SoilQualityIndex = dto.SoilQualityIndex;
        entity.CalculatedVolume = dto.CalculatedVolume;
        entity.VolumeGrowth = dto.VolumeGrowth;
        entity.LandPropertyId = dto.LandPropertyId;

        await _uow.Cadasters.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
        var updated = await _uow.Cadasters.GetWithLandPropertyAsync(id);
        return MapToDto(updated!);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await _uow.Cadasters.ExistsAsync(id)) return false;
        await _uow.Cadasters.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return true;
    }

    public Task<bool> ExistsAsync(Guid id) => _uow.Cadasters.ExistsAsync(id);

    // --- Mapping ---

    private static CadasterDto MapToDto(Cadaster c) => new()
    {
        Id = c.Id,
        CadastralNumber = c.CadastralNumber,
        ForestArea = c.ForestArea,
        ArableArea = c.ArableArea,
        GrasslandArea = c.GrasslandArea,
        YardArea = c.YardArea,
        BuildingFootprintArea = c.BuildingFootprintArea,
        UnderwaterArea = c.UnderwaterArea,
        OtherArea = c.OtherArea,
        SoilQualityIndex = c.SoilQualityIndex,
        CalculatedVolume = c.CalculatedVolume,
        VolumeGrowth = c.VolumeGrowth,
        LandPropertyId = c.LandPropertyId,
        LandPropertyName = c.LandProperty?.Name ?? string.Empty,
        ForestStands = c.ForestStands?.Select(fs => new App.DTO.ForestStand.ForestStandListDto
        {
            Id = fs.Id,
            Number = fs.Number,
            Area = fs.Area,
            TotalVolume = fs.TotalVolume,
            IsActive = fs.IsActive
        }).ToList() ?? new List<App.DTO.ForestStand.ForestStandListDto>(),
        RecentActivities = c.Activities
            ?.OrderByDescending(a => a.Date)
            .Take(5)
            .Select(a => new RecentActivityDto
            {
                Id = a.Id,
                Description = a.Description,
                Quantity = a.Quantity,
                Unit = a.Unit,
                Date = a.Date,
                ActivityTypeName = a.ActivityType?.ActivityTypeName ?? string.Empty,
                UserName = a.User?.UserName ?? string.Empty,
                CadasterCadastralNumber = c.CadastralNumber
            }).ToList() ?? new List<RecentActivityDto>()
    };

    private static CadasterListDto MapToListDto(Cadaster c) => new()
    {
        Id = c.Id,
        CadastralNumber = c.CadastralNumber,
        ForestArea = c.ForestArea,
        ForestStandCount = c.ForestStands?.Count ?? 0
    };
}
