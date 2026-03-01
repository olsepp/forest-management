using App.DTO.ActivityType;

namespace App.BLL.Services.Interfaces;

public interface IActivityTypeService
{
    Task<IEnumerable<ActivityTypeListDto>> GetAllAsync();
    Task<ActivityTypeDto?> GetByIdAsync(Guid id);
    Task<ActivityTypeDto> CreateAsync(ActivityTypeCreateDto dto);
    Task<ActivityTypeDto?> UpdateAsync(Guid id, ActivityTypeUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
