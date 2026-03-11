using App.BLL.Services.Interfaces;
using App.DAL.UnitOfWork;
using App.Domain;
using App.DTO.Activity;

namespace App.BLL.Services.Implementations;

public class ActivityService : IActivityService
{
    private readonly IUnitOfWork _uow;

    public ActivityService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<ActivityListDto>> GetAllAsync()
    {
        var activities = await _uow.Activities.GetAllWithDetailsAsync();
        return activities.Select(MapToListDto);
    }

    public async Task<ActivityDto?> GetByIdAsync(Guid id)
    {
        var activity = await _uow.Activities.GetWithDetailsAsync(id);
        return activity == null ? null : MapToDto(activity);
    }

    public async Task<IEnumerable<ActivityListDto>> GetByForestStandIdAsync(Guid forestStandId)
    {
        var activities = await _uow.Activities.GetByForestStandIdAsync(forestStandId);
        return activities.Select(MapToListDto);
    }

    public async Task<IEnumerable<ActivityListDto>> GetByCadasterIdAsync(Guid cadasterId)
    {
        var activities = await _uow.Activities.GetByCadasterIdAsync(cadasterId);
        return activities.Select(MapToListDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByCompanyIdAsync(Guid companyId)
    {
        var activities = await _uow.Activities.GetByCompanyIdAsync(companyId);
        return activities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByCompanyIdAndUserIdAsync(Guid companyId, Guid userId)
    {
        var activities = await _uow.Activities.GetByCompanyIdAndUserIdAsync(companyId, userId);
        return activities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByLandPropertyIdAsync(Guid landPropertyId)
    {
        var activities = await _uow.Activities.GetByLandPropertyIdAsync(landPropertyId);
        return activities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetByLandPropertyIdAndUserIdAsync(Guid landPropertyId, Guid userId)
    {
        var activities = await _uow.Activities.GetByLandPropertyIdAndUserIdAsync(landPropertyId, userId);
        return activities.Select(MapToDto);
    }

    public async Task<IEnumerable<RecentActivityDto>> GetRecentAsync(int count)
    {
        var activities = await _uow.Activities.GetRecentAsync(count);
        return activities.Select(MapToRecentDto);
    }

    public async Task<ActivityDto> CreateAsync(ActivityCreateDto dto, Guid userId)
    {
        var entity = new Activity
        {
            Description = dto.Description,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            Notes = dto.Notes,
            Date = dto.Date,
            ActivityTypeId = dto.ActivityTypeId,
            UserId = userId,
            ForestStandId = dto.ForestStandId,
            CadasterId = dto.CadasterId,
            ApplicationStatus = dto.ApplicationStatus
        };
        await _uow.Activities.AddAsync(entity);
        await _uow.SaveChangesAsync();
        // Reload with details for response
        var created = await _uow.Activities.GetWithDetailsAsync(entity.Id);
        return MapToDto(created!);
    }

    public async Task<ActivityDto?> UpdateAsync(Guid id, ActivityUpdateDto dto)
    {
        var entity = await _uow.Activities.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Description = dto.Description;
        entity.Quantity = dto.Quantity;
        entity.Unit = dto.Unit;
        entity.Notes = dto.Notes;
        entity.Date = dto.Date;
        entity.ActivityTypeId = dto.ActivityTypeId;
        entity.ForestStandId = dto.ForestStandId;
        entity.CadasterId = dto.CadasterId;
        entity.ApplicationStatus = dto.ApplicationStatus;

        await _uow.Activities.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
        // Reload with details for response
        var updated = await _uow.Activities.GetWithDetailsAsync(id);
        return MapToDto(updated!);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await _uow.Activities.ExistsAsync(id)) return false;
        await _uow.Activities.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return true;
    }

    public Task<bool> ExistsAsync(Guid id) => _uow.Activities.ExistsAsync(id);

    // --- Mapping ---

    private static ActivityDto MapToDto(Activity a) => new()
    {
        Id = a.Id,
        Description = a.Description,
        Quantity = a.Quantity,
        Unit = a.Unit,
        Notes = a.Notes,
        Date = a.Date,
        ActivityTypeId = a.ActivityTypeId,
        UserId = a.UserId,
        UserName = a.User?.UserName ?? string.Empty,
        ForestStandId = a.ForestStandId,
        ForestStandNumber = a.ForestStand?.Number,
        CadasterId = a.CadasterId,
        CadasterCadastralNumber = a.Cadaster?.CadastralNumber ?? a.ForestStand?.Cadaster?.CadastralNumber,
        LandPropertyId = a.Cadaster?.LandPropertyId ?? a.ForestStand?.Cadaster?.LandPropertyId,
        LandPropertyName = a.Cadaster?.LandProperty?.Name ?? a.ForestStand?.Cadaster?.LandProperty?.Name,
        ApplicationStatus = a.ApplicationStatus,
        ActivityTypeName = a.ActivityType?.ActivityTypeName ?? string.Empty
    };

    private static ActivityListDto MapToListDto(Activity a) => new()
    {
        Id = a.Id,
        Description = a.Description,
        Quantity = a.Quantity,
        Unit = a.Unit,
        Date = a.Date,
        ActivityTypeName = a.ActivityType?.ActivityTypeName ?? string.Empty,
        UserName = a.User?.UserName ?? string.Empty,
        ForestStandNumber = a.ForestStand?.Number,
        CadasterCadastralNumber = a.Cadaster?.CadastralNumber ?? a.ForestStand?.Cadaster?.CadastralNumber,
        ApplicationStatus = a.ApplicationStatus
    };

    private static RecentActivityDto MapToRecentDto(Activity a) => new()
    {
        Id = a.Id,
        Description = a.Description,
        Quantity = a.Quantity,
        Unit = a.Unit,
        Date = a.Date,
        ForestStandNumber = a.ForestStand?.Number ?? 0,
        ActivityTypeName = a.ActivityType?.ActivityTypeName ?? string.Empty,
        UserName = a.User?.UserName ?? string.Empty
    };
}
