using App.BLL.Services.Interfaces;
using App.DAL.UnitOfWork;
using App.Domain;
using App.DTO.ActivityType;

namespace App.BLL.Services.Implementations;

public class ActivityTypeService : IActivityTypeService
{
    private readonly IUnitOfWork _uow;

    public ActivityTypeService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<ActivityTypeListDto>> GetAllAsync()
    {
        var types = await _uow.ActivityTypes.GetAllAsync();
        return types.Select(MapToListDto);
    }

    public async Task<ActivityTypeDto?> GetByIdAsync(Guid id)
    {
        var type = await _uow.ActivityTypes.GetByIdAsync(id);
        return type == null ? null : MapToDto(type);
    }

    public async Task<ActivityTypeDto> CreateAsync(ActivityTypeCreateDto dto)
    {
        var entity = new ActivityType
        {
            ActivityTypeName = dto.ActivityTypeName,
        };
        await _uow.ActivityTypes.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<ActivityTypeDto?> UpdateAsync(Guid id, ActivityTypeUpdateDto dto)
    {
        var entity = await _uow.ActivityTypes.GetByIdAsync(id);
        if (entity == null) return null;

        entity.ActivityTypeName = dto.ActivityTypeName;

        await _uow.ActivityTypes.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await _uow.ActivityTypes.ExistsAsync(id)) return false;
        await _uow.ActivityTypes.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return true;
    }

    public Task<bool> ExistsAsync(Guid id) => _uow.ActivityTypes.ExistsAsync(id);

    // --- Mapping ---

    private static ActivityTypeDto MapToDto(ActivityType at) => new()
    {
        Id = at.Id,
        ActivityTypeName = at.ActivityTypeName,
    };

    private static ActivityTypeListDto MapToListDto(ActivityType at) => new()
    {
        Id = at.Id,
        ActivityTypeName = at.ActivityTypeName
    };
}
